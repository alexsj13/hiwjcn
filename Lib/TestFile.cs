using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Lib
{
    /// <summary>
    /// 关于类库任何问题（bug、优化、修改）请提交一下，3Q
    /// </summary>
    public static class ContactMe
    {
        public static readonly string Name = "wj";
        public static readonly string Email = "hiwjcn@live.com";
        public static readonly string Wechat = "hiwjcn";
        public static readonly string CodingPage = "https://github.com/hiwjcn";
    }

    public partial class PartialCls
    {
        public void xx()
        { }
    }

    public partial class PartialCls
    {
        public void xx_partial()
        { }
    }

    public class SyntaxTest
    {
        public SyntaxTest()
        {
            dynamic updateDoc = new System.Dynamic.ExpandoObject();
            updateDoc.Title = "My new title";
            updateDoc.Description = "default description";

            var bignum = 123_215_435_236_351;

            var cls = new PartialCls();
            cls.xx();
            cls.xx_partial();

            var cul = new CultureInfo("");
            var s = DateTime.Now.ToString(cul.DateTimeFormat);
        }
    }

    public class DectorTest
    {
        public int name { get; set; }
        public int age { get; set; }

        public void hold()
        {
            if (getage() < 18)
            {
                throw new Exception("");
            }
            //学python那一套，内部函数
            int getage()
            {
                return 0;
            }
        }

        public static ref int change(ref int i)
        {
            return ref i;
        }

        /// <summary>
        /// 结构函数，目前感觉好多余的功能，增加了不必要的语法糖
        /// </summary>
        /// <param name="n"></param>
        /// <param name="a"></param>
        public void Deconstruct(out int n, out int a)
        {
            n = name;
            a = age;

            Action<int> OnGetAge = async age => await Task.FromResult(1);
            OnGetAge(1);
        }
    }

    public class fasdf
    {
        public fasdf()
        {
            (int a, int b) = new DectorTest();


            int age = 10;

            var getage = DectorTest.change(ref age);
            getage = 18;
            //print age
        }
    }

    public class Test
    {
        #region 二叉树结点数据结构的定义
        //二叉树结点数据结构包括数据域，左右结点以及父结点成员；
        class nodes<T>
        {
            T data;
            nodes<T> Lnode, Rnode, Pnode;
            public T Data
            {
                set { data = value; }
                get { return data; }

            }
            public nodes<T> LNode
            {
                set { Lnode = value; }
                get { return Lnode; }
            }
            public nodes<T> RNode
            {
                set { Rnode = value; }
                get { return Rnode; }

            }

            public nodes<T> PNode
            {
                set { Pnode = value; }
                get { return Pnode; }

            }
            public nodes()
            { }
            public nodes(T data)
            {
                this.data = data;
            }

        }
        #endregion

        #region 构造一棵已知的二叉树

        static nodes<string> BinTree()
        {
            nodes<string>[] binTree = new nodes<string>[8];
            //创建结点
            binTree[0] = new nodes<string>("A");
            binTree[1] = new nodes<string>("B");
            binTree[2] = new nodes<string>("C");
            binTree[3] = new nodes<string>("D");
            binTree[4] = new nodes<string>("E");
            binTree[5] = new nodes<string>("F");
            binTree[6] = new nodes<string>("G");
            binTree[7] = new nodes<string>("H");
            //使用层次遍历二叉树的思想，构造一个已知的二叉树

            binTree[0].LNode = binTree[1];
            binTree[0].RNode = binTree[2];
            binTree[1].RNode = binTree[3];
            binTree[2].LNode = binTree[4];
            binTree[2].RNode = binTree[5];
            binTree[3].LNode = binTree[6];
            binTree[3].RNode = binTree[7];
            //返回二叉树的根结点
            return binTree[0];



        }
        #endregion


        #region 层次遍历二叉树
        static void LayerOrder<T>(nodes<T> rootNode)
        {
            nodes<T>[] Nodes = new nodes<T>[20];
            int front = -1;
            int rear = -1;
            if (rootNode != null)
            {
                rear++;
                Nodes[rear] = rootNode;

            }

            while (front != rear)
            {
                front++;
                rootNode = Nodes[front];
                Console.WriteLine(rootNode.Data);
                if (rootNode.LNode != null)
                {
                    rear++;
                    Nodes[rear] = rootNode.LNode;
                }
                if (rootNode.RNode != null)
                {
                    rear++;
                    Nodes[rear] = rootNode.RNode;
                }
            }
        }

        #endregion

        #region 测试的主方法
        static void Main(string[] args)
        {
            nodes<string> rootNode = BinTree();


            Console.WriteLine("层次遍历方法遍历二叉树：");
            LayerOrder<string>(rootNode);


            Console.Read();



        }
        public void wj()
        {
            //
        }
        private void onCon() { }

        #endregion
    }

    /// <summary>
    /// 接口中申明的必须在实现类中实现，并且都是public，自己不能实例化
    /// </summary>
    public interface IDB
    { }

    /// <summary>
    /// 抽象类和接口有点像，同样不能实例化，其中抽象函数必须被重写
    /// （包含抽象方法的类必须是抽象类）
    /// </summary>
    public abstract class ADB
    {
        /// <summary>
        /// 抽象类中可以有普通函数
        /// </summary>
        void change()
        {
            //
        }
        /// <summary>
        /// 抽象函数必须被重写(不能为private)
        /// </summary>
        public abstract void xx();
        /// <summary>
        /// 虚函数可以被重写也可以不重写（不能为private）
        /// </summary>
        public virtual void tostring()
        {
            //
        }
    }
}