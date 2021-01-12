using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace ExpressionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            test();
            return;
            Console.Write("RegualarProperty\t");
            MeasurePerformance(RegualarProperty);
            Console.Write("Reflection\t");
            MeasurePerformance(Reflection);
            Console.Write("ReflectionWithCachedPropertyInfo\t");
            MeasurePerformance(ReflectionWitchCachedPropertyInfo);
            //Console.Write("CompiledExpression\t");
            //MeasurePerformance(CompiledExpression);
            Console.Write("CachedCompiledExpression\t");
            MeasurePerformance(CachedCompiledExpression);
            Console.Write("Delegate\t");
            MeasurePerformance(Delegate);
            Console.Write("CachedDelegate\t");
            MeasurePerformance(CachedDelegate);
            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }

        private static void RegualarProperty(Person p)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000000; i++)
            {
                sb.AppendLine(p.Name.ToString());
            }
        }

        private static void Reflection(Person p)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000000; i++)
            {
                var property = p.GetType().GetProperty("Name");

                sb.AppendLine(property.GetValue(p, null).ToString());
            }
        }

        private static void ReflectionWitchCachedPropertyInfo(Person p)
        {
            var property = p.GetType().GetProperty("Name");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000000; i++)
            {
                sb.AppendLine(property.GetValue(p, null).ToString());
            }
        }

        private static void CompiledExpression(Person p)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000000; i++)
            {
                ParameterExpression arg = Expression.Parameter(p.GetType(), "x");
                Expression expr = Expression.Property(arg, "Name");

                var propertyResolver = Expression.Lambda<Func<Person, object>>(expr, arg).Compile();

                sb.AppendLine(propertyResolver(p).ToString());
            }
        }

        private static void CachedCompiledExpression(Person p)
        {
            StringBuilder sb = new StringBuilder();

            object obj = p;

            ParameterExpression arg = Expression.Parameter(obj.GetType(), "x");
            Expression expr = Expression.Property(arg, "Name");

            var propertyResolver = Expression.Lambda<Func<Person, string>>(expr, arg).Compile();

            //for (int i = 0; i < 1000000; i++)
            //{
            //    sb.AppendLine(propertyResolver(p));
            //}


            var zz = new { k = 3, Name = "pop", data = "asfddasf" };
            /*var list = new List<object>();

            for (int i = 0; i < 10; i++)
            {
                var ss = new Dog()
                {
                    Id = i,
                    Name = i.ToString(),
                };
                list.Add(ss);
            }

            Expression<Func<int, bool>> expression = x => x > 5;
            CacheCompiledExpression(list, "Id", expression);*/
        }

        class Dog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        static void test()
        {
            object d = new Dog
            {
                Id = 21,
                Name = "adsf"
            };
            genericeTest(d);
        }

        static void genericeTest<T>(T t)
        {
            var type = t.GetType();
            var arg = Expression.Parameter(typeof(T), "x");
            var expr = Expression.Property(Expression.Convert(arg, type), "Id");
            var compiled = Expression.Lambda<Func<T, int>>(expr, arg).Compile();
            var value = compiled.Invoke(t);
        }

        static IEnumerable<T> CacheCompiledExpression<T, FieldType>(IEnumerable<T> collections, string fieldName,
            Expression<Func<FieldType, bool>> condition)
        {
            var first = collections.FirstOrDefault();
            var arg = Expression.Parameter(first.GetType(), "x");
            var expr = Expression.Property(arg, fieldName);
            var valueCompiled = Expression.Lambda<Func<T, FieldType>>(expr, arg).Compile();
            var conditionCompiled = condition.Compile();
            return collections.Where(o => conditionCompiled(valueCompiled(o)));
        }

        private static Func<T, object> GetGetter<T>(T obj, string propertyName)
        {
            ParameterExpression arg = Expression.Parameter(obj.GetType(), "x");
            MemberExpression expression = Expression.Property(arg, propertyName);
            UnaryExpression conversion = Expression.Convert(expression, typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, arg).Compile();
        }

        static Expression<Action<T, string>> GetAction<T>(string fieldName)
        {
            ParameterExpression targetExpr = Expression.Parameter(typeof(T), "Target");
            MemberExpression fieldExpr = Expression.Property(targetExpr, fieldName);
            ParameterExpression valueExpr = Expression.Parameter(typeof(string), "value");
            MethodCallExpression convertExpr = Expression.Call(typeof(Convert),
                "ChangeType", null, valueExpr, Expression.Constant(fieldExpr.Type));
            UnaryExpression valueCast = Expression.Convert(convertExpr, fieldExpr.Type);
            BinaryExpression assignExpr = Expression.Assign(fieldExpr, valueCast);
            return Expression.Lambda<Action<T, string>>(assignExpr, targetExpr, valueExpr);
        }

        private static void Delegate(Person p)
        {
            for (int i = 0; i < 1000000; i++)
            {
                StringBuilder sb = new StringBuilder();

                ParameterExpression arg = Expression.Parameter(p.GetType(), "x");
                Expression expr = Expression.Property(arg, "Name");

                var propertyResolver = Expression.Lambda(expr, arg).Compile();

                sb.AppendLine(propertyResolver.DynamicInvoke(p).ToString());
            }
        }

        private static void CachedDelegate(Person p)
        {
            StringBuilder sb = new StringBuilder();

            ParameterExpression arg = Expression.Parameter(p.GetType(), "x");
            Expression expr = Expression.Property(arg, "Name");

            var propertyResolver = Expression.Lambda(expr, arg).Compile();

            for (int i = 0; i < 1000000; i++)
            {
                sb.AppendLine(propertyResolver.DynamicInvoke(p).ToString());
            }
        }

        private static void MeasurePerformance(Action<Person> action)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            action(new Person() { Name = "Test" });

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}