using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Linq.Expressions;

namespace Neo4jClient.ReturnPoly
{
    public static class PolyUtils
    {
        public static ICypherFluentQuery<T> ReturnPolymorphic<T>(this ICypherFluentQuery query, string identity)
        {
            return query.Advanced.Return<T>(new ReturnExpression()
            {
                Text = Poly(identity)
            });
        }

        public static ICypherFluentQuery<T> ReturnDistinctPolymorphic<T>(this ICypherFluentQuery query, string identity)
        {
            return query.Advanced.ReturnDistinct<T>(new ReturnExpression()
            {
                Text = Poly(identity)
            });
        }

        public static string Poly(string identity) => $"{{ Node: {identity}, Labels: labels({identity}) }}";
    }
}
