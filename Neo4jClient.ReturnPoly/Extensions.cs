using Neo4jClient;
using Neo4jClient.Cypher;
using System;

namespace Neo4JClient.ReturnPoly
{
    public static class Extensions
    {
        public static ICypherFluentQuery<T> ReturnPolymorphic<T>(this ICypherFluentQuery query, string identity)
        {
            return query.Advanced.Return<T>(new ReturnExpression()
            {
                Text = $"{{ Node: {identity}, Labels: labels({identity}) }}"
            });
        }

        public static ICypherFluentQuery<T> ReturnDistinctPolymorphic<T>(this ICypherFluentQuery query, string identity)
        {
            return query.Advanced.ReturnDistinct<T>(new ReturnExpression()
            {
                Text = $"{{ Node: {identity}, Labels: labels({identity}) }}"
            });
        }
    }
}
