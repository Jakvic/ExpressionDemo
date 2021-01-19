using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            v1();
            gen();
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
            var sb = new StringBuilder();

            for (var i = 0; i < 1000000; i++) sb.AppendLine(p.Name);
        }

        private static void Reflection(Person p)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 1000000; i++)
            {
                var property = p.GetType().GetProperty("Name");

                sb.AppendLine(property.GetValue(p, null).ToString());
            }
        }

        private static void ReflectionWitchCachedPropertyInfo(Person p)
        {
            var property = p.GetType().GetProperty("Name");

            var sb = new StringBuilder();

            for (var i = 0; i < 1000000; i++) sb.AppendLine(property.GetValue(p, null).ToString());
        }

        private static void CompiledExpression(Person p)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 1000000; i++)
            {
                var arg = Expression.Parameter(p.GetType(), "x");
                Expression expr = Expression.Property(arg, "Name");

                var propertyResolver = Expression.Lambda<Func<Person, object>>(expr, arg).Compile();

                sb.AppendLine(propertyResolver(p).ToString());
            }
        }

        private static void CachedCompiledExpression(Person p)
        {
            var sb = new StringBuilder();

            object obj = p;

            var arg = Expression.Parameter(obj.GetType(), "x");
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

        private static void v1()
        {
            var dog = new Dog {Id = 1, Name = "aa"};
            object zoo = new Zoo {Index = 111, Animal = dog};
            var mbs = zoo.GetType().GetMembers();
            var mi = RefExpr.SelectMember(mbs, info => info.Name == "Animal");
        }


        static void V2()
        {
            object dog = new Dog {Id = 1, Name = "aa"};
            object zoo = new Zoo {Index = 111, Animal = dog};
        }

        private static void gen()
        {
            object d = new Dog
            {
                Id = 21,
                Name = "app"
            };
            var model = new Zoo
            {
                Index = 1,
                Animal = d
            };

            var sb = new StringBuilder();
            Console.WriteLine("start !!");
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            for (int i = 1; i < 1000 * 1000; i++)
            {
                if (i % 2 == 0)
                {
                    var x = expTestCached(model, "Animal.Id", (int v) => v != i);
                    sb.Append(x);
                }
                else
                {
                    var y = expTestCached(d, "Name", (string v) => v != i.ToString());
                    sb.Append(y);
                }
            }

            Console.WriteLine(sw.ElapsedMilliseconds);
            sb.Clear();

            for (int i = 1; i < 1000 * 1000; i++)
            {
                if (i % 2 == 0)
                {
                    var x = expTest(model, "Animal.Id", (int v) => v != i);
                    sb.Append(x);
                }
                else
                {
                    var y = expTest(d, "Name", (string v) => v != i.ToString());
                    sb.Append(y);
                }
            }

            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private static Dictionary<string, object> cache = new();


        private static bool expTestCached<T, V>(T t, string filedName, Func<V, bool> func)
        {
            var paramExpr = Expression.Parameter(typeof(T), "x");

            Expression expr = paramExpr;
            var type = t.GetType();

            var split = filedName.Split('.');
            V tft = default;
            for (var i = 0; i < split.Length; i++)
            {
                var prop = split[i];
                expr = Expression.Property(Expression.Convert(expr, type), prop);

                if (i == split.Length - 1) // 最后一次是属性
                {
                    if (cache.TryGetValue(expr.ToString(), out var action))
                    {
                        tft = (action as Func<T, V>).Invoke(t);
                    }
                    else
                    {
                        tft = Expression.Lambda<Func<T, V>>(expr, paramExpr).Compile().Invoke(t);
                        cache[expr.ToString()] = Expression.Lambda<Func<T, V>>(expr, paramExpr).Compile();
                    }

                    break;
                }

                if (cache.TryGetValue(type.ToString(), out var o))
                {
                    type = (o as Func<T, object>)?.Invoke(t).GetType();
                }
                else
                {
                    cache[type.ToString()] = Expression.Lambda<Func<T, object>>(expr, paramExpr).Compile();
                    type = (cache[type.ToString()] as Func<T, object>).Invoke(t).GetType();
                }
            }

            return func.Invoke(tft);
        }

        private static bool expTest<T, V>(T t, string filedName, Func<V, bool> func)
        {
            var paramExpr = Expression.Parameter(typeof(T), "x");

            Expression expr = paramExpr;
            var type = t.GetType();

            var split = filedName.Split('.');
            V tft = default;
            for (var i = 0; i < split.Length; i++)
            {
                var prop = split[i];
                expr = Expression.Property(Expression.Convert(expr, type), prop);

                if (i == split.Length - 1) // 最后一次是属性
                {
                    tft = Expression.Lambda<Func<T, V>>(expr, paramExpr).Compile().Invoke(t);
                    break;
                }

                type = Expression.Lambda<Func<T, object>>(expr, paramExpr).Compile().Invoke(t).GetType();
            }

            return func.Invoke(tft);
        }

        private static IEnumerable<T> CacheCompiledExpression<T, FieldType>(IEnumerable<T> collections,
            string fieldName,
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
            var arg = Expression.Parameter(obj.GetType(), "x");
            var expression = Expression.Property(arg, propertyName);
            var conversion = Expression.Convert(expression, typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, arg).Compile();
        }

        private static Expression<Action<T, string>> GetAction<T>(string fieldName)
        {
            var targetExpr = Expression.Parameter(typeof(T), "Target");
            var fieldExpr = Expression.Property(targetExpr, fieldName);
            var valueExpr = Expression.Parameter(typeof(string), "value");
            var convertExpr = Expression.Call(typeof(Convert),
                "ChangeType", null, valueExpr, Expression.Constant(fieldExpr.Type));
            var valueCast = Expression.Convert(convertExpr, fieldExpr.Type);
            var assignExpr = Expression.Assign(fieldExpr, valueCast);
            return Expression.Lambda<Action<T, string>>(assignExpr, targetExpr, valueExpr);
        }

        private static void Delegate(Person p)
        {
            for (var i = 0; i < 1000000; i++)
            {
                var sb = new StringBuilder();

                var arg = Expression.Parameter(p.GetType(), "x");
                Expression expr = Expression.Property(arg, "Name");

                var propertyResolver = Expression.Lambda(expr, arg).Compile();

                sb.AppendLine(propertyResolver.DynamicInvoke(p).ToString());
            }
        }

        private static void CachedDelegate(Person p)
        {
            var sb = new StringBuilder();

            var arg = Expression.Parameter(p.GetType(), "x");
            Expression expr = Expression.Property(arg, "Name");

            var propertyResolver = Expression.Lambda(expr, arg).Compile();

            for (var i = 0; i < 1000000; i++) sb.AppendLine(propertyResolver.DynamicInvoke(p).ToString());
        }

        private static void MeasurePerformance(Action<Person> action)
        {
            var watch = new Stopwatch();
            watch.Start();

            action(new Person {Name = "Test"});

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        private class Dog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Zoo
        {
            public int Index { get; set; }
            public object Animal { get; set; }
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}