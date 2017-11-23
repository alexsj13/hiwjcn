using System;
using System.Linq;
using System.Linq.Expressions;
using Hiwjcn.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.User;
using System.Diagnostics;
using Lib.extension;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest4
    {
        public class node
        {
            public node left { get; set; }

            public node right { get; set; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            Expression<Func<UserModel, bool>> ex = x =>
            x.PassWord.Length == 16
            && (x.Email.Contains("gmail.com") || x.Flag > 0)
            && x.NickName == "wj";

            Expression<Func<UserModel, object>> order = x => x.NickName;

            var m = (System.Linq.Expressions.MemberExpression)order.Body;

            var body = (BinaryExpression)ex.Body;

            var left = (BinaryExpression)body.Left;
            var right = (BinaryExpression)body.Right;

            Debug.WriteLine("开始调试");
            FindExpress(left, right);
            Debug.WriteLine("结束调试");

            /*
             开始调试
AndAlso-System.Boolean
Equal-System.Boolean
OrElse-System.Boolean
GreaterThan-System.Boolean
Equal-System.Boolean
结束调试
             */
        }

        public void FindExpress(BinaryExpression left, BinaryExpression right)
        {
            if (left != null)
            {
                Debug.WriteLine($"{(ExpressionType)left.NodeType}-{left.Type}");
                FindExpress(left.Left as BinaryExpression, left.Right as BinaryExpression);
            }
            if (right != null)
            {
                Debug.WriteLine($"{(ExpressionType)right.NodeType}-{right.Type}");
                FindExpress(right.Left as BinaryExpression, right.Right as BinaryExpression);
            }
        }

        public class ConditionNode
        {
            public BinaryExpression node;

            public ConditionNode LeftNode { get; set; }
            public ConditionNode RightNode { get; set; }

            public override string ToString()
            {
                return $"({LeftNode?.ToString()} AND {RightNode?.ToString()}) AND {node?.NodeType}";
            }
        }

    }
}
