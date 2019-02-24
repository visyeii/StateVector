using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace StateVector
{
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;

    public class VectorEventBase
    {
        private enum NumberStatus : int
        {
            NOT_SET = -1
        }

        public string Head { get; set; } = string.Empty;
        public string Tail { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public Action Func { get; set; } = null;
        public int Priority { get; set; } = (int)NumberStatus.NOT_SET;
        public int Index { get; set; } = (int)NumberStatus.NOT_SET;

        public VectorEventBase()
        {

        }

        public VectorEventBase(string head, string tail, Action func)
        {
            Head = head;
            Tail = tail;
            Func = func;
        }

        public VectorEventBase(string head, string tail, string tag, Action func)
            : this(head, tail, func)
        {
            Tag = tag;
        }
    }

    public class VectorState
    {
        protected List<string> m_list = new List<string>();
        public ReadOnlyCollection<string> Collection => new ReadOnlyCollection<string>(m_list);

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
        public List<VectorEventBase> Array { get; protected set; } = new List<VectorEventBase>();

        public VectorEvent()
        {

        }

        public VectorEvent(string head, string tail, params Action[] funcArray)
           : this(head, tail, string.Empty, funcArray)
        {

        }

        public VectorEvent(VectorHead head, string tail, params Action[] funcArray)
            : this(head, tail, string.Empty, funcArray)
        {

        }

        public VectorEvent(string head, VectorTail tail, params Action[] funcArray)
            : this(head, tail, string.Empty, funcArray)
        {

        }

        public VectorEvent(VectorHead head, VectorTail tail, params Action[] funcArray)
            : this(head, tail, string.Empty, funcArray)
        {

        }

        public VectorEvent(string head, string tail, string tag, params Action[] funcArray)
        {
            Init(head, tail, tag, funcArray);
        }

        public VectorEvent(VectorHead head, string tail, string tag, params Action[] funcArray)
        {
            Init(head.Collection, tail, tag, funcArray);
        }

        public VectorEvent(string head, VectorTail tail, string tag, params Action[] funcArray)
        {
            Init(head, tail.Collection, tag, funcArray);
        }

        public VectorEvent(VectorHead head, VectorTail tail, string tag, params Action[] funcArray)
        {
            Init(head.Collection, tail.Collection, tag, funcArray);
        }

        public static VectorHead HeadOr(params string[] head)
        {
            return new VectorHead(head);
        }

        public static VectorTail TailOr(params string[] tail)
        {
            return new VectorTail(tail);
        }

        public static Action Func(Action func)
        {
            return func;
        }

        public static Action[] FuncArray(params Action[] funcArray)
        {
            return funcArray;
        }

        protected void Init(IList<string> headCollection, IList<string> tailCollection, string tag, params Action[] funcArray)
        {
            foreach (var head in headCollection)
            {
                if (string.IsNullOrEmpty(head))
                {
                    throw new ArgumentException("head array contains \"\"");
                }

                Init(head, tailCollection, tag, funcArray);
            }
        }

        protected void Init(IList<string> headCollection, string tail, string tag, params Action[] funcArray)
        {
            foreach (var head in headCollection)
            {
                if (string.IsNullOrEmpty(head))
                {
                    throw new ArgumentException("head array contains \"\"");
                }

                Init(head, tail, tag, funcArray);
            }
        }

        protected void Init(string head, IList<string> tailCollection, string tag, params Action[] funcArray)
        {
            foreach (var tail in tailCollection)
            {
                if (string.IsNullOrEmpty(tail))
                {
                    throw new ArgumentException("tail array contains \"\"");
                }

                Init(head, tail, tag, funcArray);
            }
        }

        protected void Init(string head, string tail, string tag, params Action[] funcArray)
        {
            if (string.IsNullOrEmpty(head))
            {
                throw new ArgumentNullException(nameof(head));
            }

            if (string.IsNullOrEmpty(tail))
            {
                throw new ArgumentNullException(nameof(tail));
            }

            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            foreach (var func in funcArray)
            {
                if (func == null)
                {
                    throw new ArgumentNullException(nameof(func));
                }

                Array.Add(new VectorEventBase(head, tail, tag, func));
            }
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
        public bool EnableRefreshTrace { get; set; } = false;
        public bool EnableRegexp { get; set; } = false;
        protected List<VectorEventBase> EventList { get; set; }  = new List<VectorEventBase>();

        public string StateNow { get; set; } = string.Empty;
        public string StateOld { get; protected set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;

        public StateVector()
        {

        }

        public StateVector(string startState, params VectorEvent[] eventArray)
        {
            Init(startState, eventArray);
        }

        public StateVector(string listName, string startState, params VectorEvent[] eventArray)
            : this(startState, eventArray)
        {
            ListName = listName;
        }

        protected void Init(string startState, VectorEvent[] eventArray)
        {
            int index = 0;
            int prioroty = 0;
            StateNow = startState;
            EventList.Clear();

            foreach (var ve in eventArray)
            {
                foreach (var veb in ve.Array)
                {
                    veb.Index = index;
                    veb.Priority = prioroty;
                    EventList.Add(veb);
                    prioroty++;
                }
                index++;
            }
        }

        public void GetListInfo()
        {
            foreach (var vbe in EventList)
            {
                Debug.WriteLine($"{ListName} : { vbe.Tag } "
                    + $"list[{ vbe.Index }].priority({ vbe.Priority }) {vbe.Head} -> {vbe.Tail} , {vbe.Func.Method.Name}");
            }
        }

        public void Refresh(string stateNext)
        {
            List<VectorEventBase> list = new List<VectorEventBase>();

            if (EnableRegexp)
            {
                list = GetRegexp(StateNow, stateNext);
            }
            else
            {
                list = GetHeadAndTali(StateNow, stateNext);
            }

            foreach (var vbe in list)
            {
                if (EnableRefreshTrace)
                {
                    Debug.WriteLine($"{ListName} {vbe.Tag} {StateNow} -> {stateNext} "
                        + $"do[{vbe.Index}].priority({vbe.Priority}) {vbe.Func.Method.Name}");
                }

                vbe.Func();

                if (EnableRefreshTrace)
                {
                    Debug.WriteLine(" done.");
                }
            }

            StateOld = StateNow;
            StateNow = stateNext;
        }

        protected List<VectorEventBase> GetHeadAndTali(string stateNow, string stateNext)
        {
            return EventList
                .Where(veBase => (veBase.Head == stateNow && veBase.Tail == stateNext))
                .ToList();
        }

        protected List<VectorEventBase> GetRegexp(string stateNow, string stateNext)
        {
            return EventList
                .Where(veBase => (
                    Regex.IsMatch(stateNow, veBase.Head) && Regex.IsMatch(stateNext, veBase.Tail)))
                .ToList();
        }
    }
}
