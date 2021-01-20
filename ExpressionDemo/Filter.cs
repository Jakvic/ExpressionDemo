using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionDemo
{
    public class Filter
    {
        private readonly Dictionary<string, object> cache = new();

        public bool FilterByFunc<T, V>(T t, string filedName, Func<V, bool> func)
        {
            var paramExpr = Expression.Parameter(typeof(T), "x");

            Expression expr = paramExpr;
            var type = t.GetType();

            var split = filedName.Split('.');
            V v = default;
            for (var i = 0; i < split.Length; i++)
            {
                var prop = split[i];
                expr = Expression.Property(Expression.Convert(expr, type!), prop);

                if (i == split.Length - 1) // 最后一次是属性
                {
                    if (cache.TryGetValue(expr.ToString(), out var action))
                    {
                        v = (action as Func<T, V>)!.Invoke(t);
                    }
                    else
                    {
                        var valueCompiled = Expression.Lambda<Func<T, V>>(expr, paramExpr).Compile();
                        cache[expr.ToString()] = valueCompiled;
                        v = valueCompiled.Invoke(t);
                    }

                    break;
                }

                if (cache.TryGetValue(type.ToString(), out var o))
                {
                    type = (o as Func<T, object>)?.Invoke(t).GetType();
                }
                else
                {
                    var typeCompiled = Expression.Lambda<Func<T, object>>(expr, paramExpr).Compile();
                    cache[type.ToString()] = typeCompiled;
                    type = (cache[type.ToString()] as Func<T, object>)!.Invoke(t).GetType();
                }
            }

            return func.Invoke(v);
        }
    }
}