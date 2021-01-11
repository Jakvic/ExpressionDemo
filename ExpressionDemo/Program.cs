using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace ExpressionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("RegualarProperty\t");
            MeasurePerformance(RegualarProperty);
            Console.Write("Reflection\t");
            MeasurePerformance(Reflection);
            Console.Write("ReflectionWithCachedPropertyInfo\t");
            MeasurePerformance(ReflectionWitchCachedPropertyInfo);
            Console.Write("CompiledExpression\t");
            MeasurePerformance(CompiledExpression);
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

            ParameterExpression arg = Expression.Parameter(p.GetType(), "x");
            Expression expr = Expression.Property(arg, "Name");

            var propertyResolver = Expression.Lambda<Func<Person, object>>(expr, arg).Compile();

            for (int i = 0; i < 1000000; i++)
            {
                sb.AppendLine(propertyResolver(p).ToString());
            }
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
