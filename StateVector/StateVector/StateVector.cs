using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace StateVector
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class VectorEventBase
    {
        public enum NumberStatus : int
        {
            NOT_SET = -1
        }

        public static readonly Action FUNC_NOT_SET = null;

        public string Head { get; set; } = string.Empty;
        public string Tail { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public Action Func { get; set; } = FUNC_NOT_SET;
        public int Priority { get; set; } = (int)NumberStatus.NOT_SET;
        public int Index { get; set; } = (int)NumberStatus.NOT_SET;

        public VectorEventBase()
        {

        }

        public VectorEventBase(string head, string tail, Action func)
            : this(head, tail, string.Empty, func)
        {

        }

        public VectorEventBase(string head, string tail, string tag, Action func)
        {
            Tag = tag;
            Head = head;
            Tail = tail;
            Func = func;
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

        public VectorState(VectorState vectorState)
        {
            m_list.AddRange(vectorState.Collection);
        }
    }

    public class VectorHead : VectorState
    {
        public VectorHead(params string[] stateList)
            : base(stateList)
        {

        }

        public VectorHead(VectorHead vectorHead)
            : base(vectorHead)
        {

        }
    }

    public class VectorTail : VectorState
    {
        public VectorTail(params string[] stateList)
            : base(stateList)
        {

        }

        public VectorTail(VectorTail vectorTail)
            : base(vectorTail)
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
            : this((new VectorHead(head)), (new VectorTail(tail)), string.Empty, funcArray)
        {

        }

        public VectorEvent(VectorHead head, string tail, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), string.Empty, funcArray)
        {

        }

        public VectorEvent(string head, VectorTail tail, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), string.Empty, funcArray)
        {

        }

        public VectorEvent(VectorHead head, VectorTail tail, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), string.Empty, funcArray)
        {

        }

        public VectorEvent(string head, string tail, string tag, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), tag, funcArray)
        {

        }

        public VectorEvent(VectorHead head, string tail, string tag, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), tag, funcArray)
        {

        }

        public VectorEvent(string head, VectorTail tail, string tag, params Action[] funcArray)
            : this((new VectorHead(head)), (new VectorTail(tail)), tag, funcArray)
        {

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
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            foreach (var func in funcArray)
            {
                if (func == VectorEventBase.FUNC_NOT_SET)
                {
                    throw new ArgumentNullException(nameof(func));
                }

                Array.Add(new VectorEventBase(head, tail, tag, func));
            }
        }
    }

    public class StateVectorTraceInfo : VectorEventBase
    {
        public bool IsHit { get; set; } = false;
        public string ListName { get; set; } = string.Empty;
        public MethodInfo FuncInfo { get; set; } = null;
        public bool IsDone { get; internal set; } = false;

        public StateVectorTraceInfo()
        {

        }
    }

    public class StateVector
    {
        public static readonly Exception NO_EXCEPTION = null;
        public static readonly Func<StateVectorTraceInfo, Exception> FUNC_NOT_SET = null;
        public bool EnableRefreshTrace { get; set; } = false;
        public bool EnableRegexp { get; set; } = false;
        protected List<VectorEventBase> EventList { get; set; } = new List<VectorEventBase>();
        public Func<StateVectorTraceInfo, Exception> TraceFunc { get; set; } = new Func<StateVectorTraceInfo, Exception>(
            (StateVectorTraceInfo traceInfo) => {// example
                Exception ex = null;

                string msg = $"{traceInfo.ListName} {traceInfo.Tag} {traceInfo.Head} -> {traceInfo.Tail} "
                            + $"do[{traceInfo.Index}].priority({traceInfo.Priority}) "
                            + $"{(traceInfo.FuncInfo == null ? "" : traceInfo.FuncInfo.Name)}";

                if (traceInfo.IsHit)
                {
                    if (traceInfo.IsDone)
                    {
                        Debug.WriteLine(msg);
                    }
                    else
                    {
                        Debug.WriteLine(" done.");
                    }
                }
                else
                {
                    ex = new NotImplementedException(msg);
                }

                return ex;
            });

        public string StateNow { get; set; } = string.Empty;
        public string StateOld { get; protected set; } = string.Empty;
        public string ListName { get; set; } = string.Empty;

        public StateVector()
        {

        }

        public StateVector(string startState, params VectorEvent[] eventArray)
            : this(string.Empty, startState, FUNC_NOT_SET, eventArray)
        {

        }

        public StateVector(string listName, string startState, params VectorEvent[] eventArray)
            : this(listName, startState, FUNC_NOT_SET, eventArray)
        {

        }

        public StateVector(
            string startState,
            Func<StateVectorTraceInfo, Exception> traceFunc,
            params VectorEvent[] eventArray)
            : this(string.Empty, startState, traceFunc, eventArray)
        {

        }

        public StateVector(
            string listName,
            string startState,
            Func<StateVectorTraceInfo, Exception> traceFunc,
            params VectorEvent[] eventArray)
        {
            if (traceFunc != FUNC_NOT_SET)
            {
                TraceFunc = traceFunc;
            }

            ListName = listName;
            Init(startState, eventArray);
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

        public void GetEventList(Func<StateVectorTraceInfo, Exception> onceTraceFunc = null)
        {
            foreach (var vbe in EventList)
            {
                var traceInfo = SetTraceInfo(vbe.Head, vbe.Tail, vbe);

                traceInfo.IsHit = true;

                if (onceTraceFunc != FUNC_NOT_SET)
                {
                    Trace(onceTraceFunc, traceInfo);
                }
                else
                {
                    Trace(TraceFunc, traceInfo);
                }
            }
        }

        public void Refresh(string stateNext, Func<StateVectorTraceInfo, Exception> onceTraceFunc = null)
        {
            List<VectorEventBase> list = GetNextEventList(stateNext);

            foreach (var vbe in list)
            {
                var traceInfo = SetTraceInfo(StateNow, stateNext, vbe);

                traceInfo.IsHit = true;

                if (EnableRefreshTrace)
                {
                    Trace(TraceFunc, traceInfo);//start
                }

                Trace(onceTraceFunc, traceInfo);//start

                vbe.Func();

                traceInfo.IsDone = true;

                if (EnableRefreshTrace)
                {
                    Trace(TraceFunc, traceInfo);//end
                }

                Trace(onceTraceFunc, traceInfo);//end
            }

            if (list.Count == 0)
            {
                var traceInfo = SetTraceInfo(StateNow, stateNext);

                Trace(TraceFunc, traceInfo);
                Trace(onceTraceFunc, traceInfo);
            }

            StateOld = StateNow;
            StateNow = stateNext;
        }

        protected StateVectorTraceInfo SetTraceInfo(string stateNow, string stateNext, VectorEventBase vbe = null)
        {
            StateVectorTraceInfo traceInfo = new StateVectorTraceInfo();

            traceInfo.IsHit = false;
            traceInfo.ListName = ListName;
            traceInfo.Head = stateNow;
            traceInfo.Tail = stateNext;

            if (vbe != null)
            {
                traceInfo.Tag = vbe.Tag;
                traceInfo.Index = vbe.Index;
                traceInfo.Priority = vbe.Priority;
                traceInfo.FuncInfo = vbe.Func.Method;
            }

            traceInfo.IsDone = false;

            return traceInfo;
        }

        protected void Trace(Func<StateVectorTraceInfo, Exception> traceFunc, StateVectorTraceInfo info)
        {
            var ex = traceFunc?.Invoke(info);

            if (ex != NO_EXCEPTION)
            {
                throw ex;
            }
        }

        public List<VectorEventBase> GetNextEventList(string stateNext)
        {
            var list = new List<VectorEventBase>();

            if (EnableRegexp)
            {
                list = GetRegexp(StateNow, stateNext);
            }
            else
            {
                list = GetHeadAndTali(StateNow, stateNext);
            }

            return list;
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
