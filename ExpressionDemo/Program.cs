using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace ExpressionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            v1();
            return;
            gen();

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


            var zz = new {k = 3, Name = "pop", data = "asfddasf"};
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

        class Zoo
        {
            public int Index { get; set; }
            public object Animal { get; set; }
        }

        static void v1()
        {
            var dog = new Dog {Id = 1, Name = "aa"};
            object zoo = new Zoo {Index = 111, Animal = dog};
            var mbs = zoo.GetType().GetMembers();
            var mi = RefExpr.SelectMember(mbs, info => info.Name == "Animal");
        }

        static void gen()
        {
            object d = new Dog
            {
                Id = 21,
                Name = "app"
            };
            var model = new Zoo
            {
                Index = 1,
                Animal = d,
            };

            var x = expTest(model, "Data.Id", (int v) => v != 1);
        }

        static bool expTest<T, V>(T t, string filedName, Func<V, bool> func)
        {
            var parameterExpression = Expression.Parameter(typeof(T), "x");
            Expression expr = parameterExpression;
            var type = expr.Type;

            var split = filedName.Split('.');
            V tft = default;
            for (var i = 0; i < split.Length; i++)
            {
                var prop = split[i];
                if (i == split.Length - 1) // 最后一次是属性
                {
                    tft = Expression.Lambda<Func<T, V>>(expr, parameterExpression).Compile().Invoke(t);
                    break;
                }

                expr = Expression.Property(Expression.Convert(expr, type), prop);
                type = Expression.Lambda<Func<T, object>>(expr, parameterExpression).Compile().Invoke(t).GetType();
            }

            return func.Invoke(tft);
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

            action(new Person() {Name = "Test"});

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}