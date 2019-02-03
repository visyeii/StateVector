using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateVector.Tests
{
    using TW_StateVector = TestWrapper_StateVector;
    using VEB = VectorEventBase;
    using VE = VectorEvent;

    public class TestWrapper_StateVector : StateVector
    {
        public TestWrapper_StateVector()
            : base()
        {

        }

        public TestWrapper_StateVector(string startState, VE[] eventArray)
            : base(startState, eventArray)
        {

        }
        public TestWrapper_StateVector(string listName, string startState, VE[] eventArray)
            : base(listName, startState, eventArray)
        {

        }

        public new List<VEB> GetHeadAndTali(string head, string tail)
        {
            return base.GetHeadAndTali(head, tail);
        }
    }

    [TestClass()]
    public class StateVectorTests
    {
        TW_StateVector ins = new TW_StateVector();

        [TestMethod()]
        public void GetHeadAndTali_ret1()
        {
            VE[] list = {
                new VE("a", "b", () => { }),
                new VE("b", "a", () => { }),
                new VE("a", "a", () => { }),
                new VE("b", "b", () => { })
            };
            ins = new TW_StateVector("a", list);

            List<VEB> ret = new List<VEB>();
            ret = ins.GetHeadAndTali("a", "b");
            Assert.AreEqual(1, ret.Count);
        }

        [TestMethod()]
        public void GetHeadAndTali_ret2()
        {
            VE[] list = {
                new VE("a", "b", () => { /* 1st Hit */ }),
                new VE("b", "a", () => { }),
                new VE("a", "a", () => { }),
                new VE("b", "b", () => { }),
                new VE("a", "b", () => { /* 2nd Hit */ })
            };
            ins = new TW_StateVector("a", list);

            List<VEB> ret = new List<VEB>();
            ret = ins.GetHeadAndTali("a", "b");
            Assert.AreEqual(2, ret.Count);
        }

        [TestMethod()]
        public void GetHeadAndTali_ret2_tag()
        {
            VE[] list = {
                new VE("a", "b", () => { /* 1st Hit */ }),
                new VE("b", "a", () => { }),
                new VE("a", "a", () => { }),
                new VE("b", "b", () => { }),
                new VE("a", "b", "tag", () => { /* 2nd Hit */ })
            };
            ins = new TW_StateVector("a", list);

            List<VEB> ret = new List<VEB>();
            ret = ins.GetHeadAndTali("a", "b");
            Assert.AreEqual(2, ret.Count);
        }

        void NameCheckFunc()
        {

        }

        [TestMethod()]
        public void Refresh_OK_ab()
        {
            // 状態Aから状態Bへの変化条件が登録済みのため動作する
            int lambdaCheck = 0;

            Action func = () => { lambdaCheck = 1; };

            VE[] list = {
                new VE("a", "b", () => { lambdaCheck = 1; }),
                new VE("a", "b", func),
                new VE("a", "b", NameCheckFunc)
            };
            ins = new TW_StateVector("Refresh_OK_ab", "a", list);
            ins.EnableRefreshTrace = true;
            ins.Refresh("b");
            Assert.AreEqual(1, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_OK_regexp()
        {
            int lambdaCheck = 0;

            Action func = () => { lambdaCheck = 1; };//<Refresh_OK_ab>g__func|0

            VE[] list = {
                new VE(@".*", @"[bc]", func)
            };
            ins = new TW_StateVector("a", list);
            ins.EnableRefreshTrace = true;
            ins.EnableRegexp = true;
            ins.Refresh("b");
            Assert.AreEqual(1, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_head_null()
        {
            int lambdaCheck = 0;
            bool isCatch = false;

            Action func = () => { lambdaCheck = 1; };
            try
            {
                VE[] list = {
                    new VE((string)null, "b", func),
                };
                ins = new TW_StateVector("a", list);
            }
            catch (ArgumentNullException)
            {
                isCatch = true;
            }

            Assert.AreEqual(true, isCatch);
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_tail_null()
        {
            int lambdaCheck = 0;
            bool isCatch = false;

            Action func = () => { lambdaCheck = 1; };
            try
            {
                VE[] list = {
                    new VE("a", (string)null, func),
                };
                ins = new TW_StateVector("a", list);
            }
            catch (ArgumentNullException)
            {
                isCatch = true;
            }

            Assert.AreEqual(true, isCatch);
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_func_null()
        {
            int lambdaCheck = 0;
            bool isCatch = false;

            Action func = () => { lambdaCheck = 1; };
            try
            {
                VE[] list = {
                    new VE("a", "b", (Action)null),
                };
                ins = new TW_StateVector("a", list);
            }
            catch (ArgumentNullException)
            {
                isCatch = true;
            }

            Assert.AreEqual(true, isCatch);
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_OK_ab_FuncList()
        {
            // 状態Aから状態Bへの変化条件が登録済みのため動作する
            int lambdaCheck = 0;

            Action func = () => { lambdaCheck = 1; };//<Refresh_OK_ab>g__func|0

            VE[] list = {
                new VE("a", "b",
                    VE.Func(() => { lambdaCheck = 1; }), /* <Refresh_OK_ab>b__1 */
                    func,                       /* <Refresh_OK_ab_FuncList>b__0 */
                    NameCheckFunc,              /* NameCheckFunc */
                    () => { lambdaCheck = 1; }  /* <Refresh_OK_ab_FuncList>b__2 */)
            };
            ins = new TW_StateVector("Refresh_OK_ab_FuncList", "a", list);
            ins.EnableRefreshTrace = true;
            ins.Refresh("b");
            Assert.AreEqual(1, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_OK_ab_bb()
        {
            // 状態A or Bから状態Bへの変化条件が登録済みのため動作する
            int lambdaCheck = 0;

            Action func = () => { lambdaCheck++; };

            VE[] list = {
                new VE(
                    VE.HeadOr( "a", "b" ),
                    "b",
                    VE.Func(func))
            };
            ins = new TW_StateVector("Refresh_OK_ab_bb", "a", list);
            ins.EnableRefreshTrace = true;
            ins.Refresh("b");
            Assert.AreEqual(1, lambdaCheck);
            ins.Refresh("b");
            Assert.AreEqual(2, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_OK_ab_ac()
        {
            // 状態Aから状態B or Cへの変化条件が登録済みのため動作する
            int lambdaCheck = 0;

            Action func = () => { lambdaCheck++; };

            VE[] list = {
                new VE(
                    "a",
                    VE.TailOr("b", "c" ),
                    func)
            };
            ins = new TW_StateVector("Refresh_OK_ab_ac", "a", list);
            ins.EnableRefreshTrace = true;
            ins.Refresh("b");
            Assert.AreEqual(1, lambdaCheck);
            ins.StateNow = "a";
            ins.Refresh("c");
            Assert.AreEqual(2, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_aa()
        {
            // 状態Aから状態Aへの変化条件(状態A維持)は未登録のため動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);
            ins.Refresh("a");
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_bb()
        {
            // 状態Bから状態Bへの変化条件(状態B維持)は未登録のため動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);

            ins.StateNow = ("b");
            ins.Refresh("b");
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_ba()
        {
            // 状態Bから状態Aへの変化条件は未登録のため動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);

            ins.StateNow = ("b");
            ins.Refresh("a");
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_bc()
        {
            //条件を追加していない状態Cでは動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);

            ins.StateNow = ("b");
            ins.Refresh("c");
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_cb()
        {
            //条件を追加していない状態Cでは動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);

            ins.StateNow = ("c");
            ins.Refresh("b");
            Assert.AreEqual(0, lambdaCheck);
        }

        [TestMethod()]
        public void Refresh_NG_cc()
        {
            //条件を追加していない状態Cでは動作しない
            int lambdaCheck = 0;
            VE[] list = {
                new VE("a", "b", /* ラムダ式 */() => { lambdaCheck = 1; })
            };
            ins = new TW_StateVector("a", list);

            ins.StateNow = ("c");
            ins.Refresh("b");
            Assert.AreEqual(0, lambdaCheck);
        }
    }
}