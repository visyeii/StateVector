using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace StateVector
{
    using System.Text.RegularExpressions;
    using VEFD = VectorEventFuncDelegate;

    public delegate void VectorEventFuncDelegate();

    public class VectorEventBase
    {

        protected string m_head = "";
        protected string m_tail = "";
        protected string m_tag = "";
        protected VEFD m_func;
        protected int m_listPriority = -1;
        protected int m_listIndex = -1;

        public string Head
        {
            get
            {
                return m_head;
            }

            set
            {
                m_head = value;
            }
        }

        public string Tail
        {
            get
            {
                return m_tail;
            }

            set
            {
                m_tail = value;
            }
        }

        public string Tag
        {
            get
            {
                return m_tag;
            }

            set
            {
                m_tag = value;
            }
        }

        public VEFD Func
        {
            get
            {
                return m_func;
            }

            set
            {
                m_func = value;
            }
        }

        public int Priority
        {
            get
            {
                return m_listPriority;
            }

            set
            {
                m_listPriority = value;
            }
        }

        public int Index
        {
            get
            {
                return m_listIndex;
            }

            set
            {
                m_listIndex = value;
            }
        }

        public VectorEventBase()
        {

        }

        public VectorEventBase(string head, string tail, VEFD func)
        {
            Init(head, tail, func);
        }

        public VectorEventBase(string head, string tail, string tag, VEFD func)
        {
            Tag = tag;
            Init(head, tail, func);
        }

        protected void Init(string head, string tail, VEFD func)
        {
            m_head = head;
            m_tail = tail;
            m_func = func;
        }
    }

    public class VectorState
    {
        protected List<string> m_list = new List<string>();

        public string[] Array
        {
            get
            {
                return m_list.ToArray();
            }
        }

        public VectorState(params string[] stateList)
        {
            m_list.AddRange(stateList);
        }
    }

    public class VectorHead : VectorState
    {
        public VectorHead(params string[] stateList)
            : base(stateList)
        {

        }
    }

    public class VectorTail : VectorState
    {
        public VectorTail(params string[] stateList)
            : base(stateList)
        {

        }
    }

    public class VectorEvent
    {

        protected List<VectorEventBase> m_vectorEventList = new List<VectorEventBase>();

        public VectorEventBase[] Array
        {
            get
            {
                return m_vectorEventList.ToArray();
            }
        }

        public VectorEvent()
        {

        }

        public VectorEvent(string head, string tail, params VEFD[] funcArray)
        {
            Init(head, tail, "", funcArray);
        }

        public VectorEvent(VectorHead head, string tail, params VEFD[] funcArray)
        {
            Init(head.Array, tail, "", funcArray);
        }

        public VectorEvent(string head, VectorTail tail, params VEFD[] funcArray)
        {
            Init(head, tail.Array, "", funcArray);
        }

        public VectorEvent(VectorHead head, VectorTail tail, params VEFD[] funcArray)
        {
            Init(head.Array, tail.Array, "", funcArray);
        }

        public VectorEvent(string head, string tail, string tag, params VEFD[] funcArray)
        {
            Init(head, tail, tag, funcArray);
        }

        public VectorEvent(VectorHead head, string tail, string tag, params VEFD[] funcArray)
        {
            Init(head.Array, tail, tag, funcArray);
        }

        public VectorEvent(string head, VectorTail tail, string tag, params VEFD[] funcArray)
        {
            Init(head, tail.Array, tag, funcArray);
        }

        public VectorEvent(VectorHead head, VectorTail tail, string tag, params VEFD[] funcArray)
        {
            Init(head.Array, tail.Array, tag, funcArray);
        }

        public static VectorHead HeadOr(params string[] head)
        {
            return new VectorHead(head);
        }

        public static VectorTail TailOr(params string[] tail)
        {
            return new VectorTail(tail);
        }

        public static VEFD Func(VEFD func)
        {
            return func;
        }

        public static VEFD[] FuncArray(params VEFD[] funcArray)
        {
            return funcArray;
        }

        protected void Init(string[] headArray, string[] tailArray, string tag, params VEFD[] funcArray)
        {
            foreach (string head in headArray)
            {
                if (head == "")
                {
                    throw new ArgumentException("head array contains \"\"");
                }

                Init(head, tailArray, tag, funcArray);
            }
        }

        protected void Init(string[] headArray, string tail, string tag, params VEFD[] funcArray)
        {
            foreach (string head in headArray)
            {
                if (head == "")
                {
                    throw new ArgumentException("head array contains \"\"");
                }

                Init(head, tail, tag, funcArray);
            }
        }

        protected void Init(string head, string[] tailArray, string tag, params VEFD[] funcArray)
        {
            foreach (string tail in tailArray)
            {
                if (tail == "")
                {
                    throw new ArgumentException("tail array contains \"\"");
                }

                Init(head, tail, tag, funcArray);
            }
        }

        protected void Init(string head, string tail, string tag, params VEFD[] funcArray)
        {
            foreach(VEFD func in funcArray)
            {
                if (IsNullArgument(head, tail, tag, func))
                {
                    throw new ArgumentNullException();
                }

                m_vectorEventList.Add(new VectorEventBase(head, tail, tag, func));
            }
        }

        protected bool IsNullArgument(params object[] objArray)
        {
            bool ret = false;

            foreach(object obj in objArray)
            {
                if (obj == null)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }

    /// <summary>
    /// 本クラスは関数テーブルの使い勝手向上を目的とするクラスである。
    /// コンストラクタで状態変化時に実行する処理を集約して登録する。
    /// 
    /// 関数テーブルでは、組み合わせ増加に応じてテーブルサイズや次元数が増加する。
    /// しかし、実際に使用される状態変化条件は僅かである。
    /// また、条件変更時の書き換え作業が煩雑になる傾向がある。
    /// 
    /// 動的な関数テーブル変更は非推奨。
    /// GUIの状態変化を想定しているため、
    /// 10ミリ秒単位の精度が要求される高速な状態変化の制御には不適切。
    /// </summary>
    public class StateVector
    {
        public bool EnableRefreshTrace = false;
        public bool EnableRegexp = false;
        protected string m_stateNow;
        protected string m_stateOld;
        protected string m_listName;
        protected List<VectorEventBase> m_eventList = new List<VectorEventBase>();
        
        public string StateNow
        {
            get
            {
                return m_stateNow;
            }

            set
            {
                m_stateNow = value;
            }
        }

        public string StateOld
        {
            get
            {
                return m_stateOld;
            }
        }

        public string ListName
        {
            get
            {
                return m_listName;
            }

            set
            {
                m_listName = value;
            }
        }

        public StateVector()
        {

        }
        
        public StateVector(string startState, VectorEvent[] eventArray)
        {
            Init(startState, eventArray);
        }

        public StateVector(string listName, string startState, VectorEvent[] eventArray)
        {
            m_listName = listName;
            Init(startState, eventArray);
        }

        protected void Init(string startState, VectorEvent[] eventArray)
        {
            int index = 0;
            int prioroty = 0;
            StateNow = startState;
            m_eventList.Clear();

            foreach (VectorEvent ve in eventArray)
            {
                foreach (VectorEventBase ins in ve.Array)
                {
                    ins.Index = index;
                    ins.Priority = prioroty;
                    m_eventList.Add(ins);
                    prioroty++;
                }
                index++;
            }
        }

        public void GetListInfo()
        {
            foreach(VectorEventBase ins in m_eventList)
            {
                Debug.Write(m_listName + ":");
                Debug.WriteLine(GetEventSetting(ins));
            }
        }

        protected string GetEventSetting(VectorEventBase ins)
        {
            string ret = "";

            ret = ins.Tag + " list[" + ins.Index +"].priority("+ ins.Priority + ") "
                + ins.Head + " -> " + ins.Tail + " , " + ins.Func.Method.Name;

            return ret;
        }

        public void Refresh(string stateNext)
        {
            List<VectorEventBase> list = new List<VectorEventBase>();

            if (EnableRegexp)
            {
                list = GetRegexp(m_stateNow, stateNext);
            }
            else
            {
                list = GetHeadAndTali(m_stateNow, stateNext);
            }

            foreach (VectorEventBase ins in list)
            {
                if (EnableRefreshTrace)
                {
                    Debug.Write(m_listName + " " + ins.Tag + " ");
                    Debug.Write(m_stateNow + " -> " + stateNext);
                    Debug.Write(" do[" + ins.Index + "].priority(" + ins.Priority + ") " + ins.Func.Method.Name);
                }

                ins.Func();

                if (EnableRefreshTrace)
                {
                    Debug.WriteLine(" done.");
                }
            }

            m_stateOld = m_stateNow;
            m_stateNow = stateNext;
        }

        protected　List<VectorEventBase> GetHeadAndTali(string stateNow, string stateNext)
        {
            List<VectorEventBase> ret = new List<VectorEventBase>();

            foreach (VectorEventBase ins in m_eventList)
            {
                if (stateNow == ins.Head)
                {
                    if (stateNext == ins.Tail)
                    {
                        ret.Add(ins);
                    }
                }
            }

            return ret;
        }

        protected List<VectorEventBase> GetRegexp(string stateNow, string stateNext)
        {
            List<VectorEventBase> ret = new List<VectorEventBase>();

            foreach (VectorEventBase ins in m_eventList)
            {
                if (Regex.IsMatch(stateNow, ins.Head))
                {
                    if (Regex.IsMatch(stateNext, ins.Tail))
                    {
                        ret.Add(ins);
                    }
                }
            }

            return ret;
        }
    }
}
