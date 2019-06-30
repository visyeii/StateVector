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

        public const Action FUNC_NOT_SET = null;

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

    public static class VectorEventExtension
    {
        public static void Add(this List<VectorEvent> list, string head, string tail, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, funcArray));

        public static void Add(this List<VectorEvent> list, VectorHead head, string tail, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, funcArray));

        public static void Add(this List<VectorEvent> list, string head, VectorTail tail, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, funcArray));

        public static void Add(this List<VectorEvent> list, VectorHead head, VectorTail tail, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, funcArray));

        public static void Add(this List<VectorEvent> list, string head, string tail, string tag, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, tag, funcArray));

        public static void Add(this List<VectorEvent> list, VectorHead head, string tail, string tag, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, tag, funcArray));

        public static void Add(this List<VectorEvent> list, string head, VectorTail tail, string tag, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, tag, funcArray));

        public static void Add(this List<VectorEvent> list, VectorHead head, VectorTail tail, string tag, params Action[] funcArray)
            => list.Add(new VectorEvent(head, tail, tag, funcArray));
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
        public const Exception NO_EXCEPTION = null;
        public const Func<StateVectorTraceInfo, Exception> FUNC_NOT_SET = null;
        public bool EnableRefreshTrace { get; set; } = true;
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
                        Debug.WriteLine(" done.");
                    }
                    else
                    {
                        Debug.WriteLine(msg);
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

        public StateVector(string startState, List<VectorEvent> eventList)
            : this(string.Empty, startState, FUNC_NOT_SET, eventList.ToArray())
        {

        }

        public StateVector(string listName, string startState, params VectorEvent[] eventArray)
            : this(listName, startState, FUNC_NOT_SET, eventArray)
        {

        }

        public StateVector(string listName, string startState, List<VectorEvent> eventList)
            : this(listName, startState, FUNC_NOT_SET, eventList.ToArray())
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
            string startState,
            Func<StateVectorTraceInfo, Exception> traceFunc,
            List<VectorEvent> eventList)
            : this(string.Empty, startState, traceFunc, eventList.ToArray())
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

        public StateVector(
            string listName,
            string startState,
            Func<StateVectorTraceInfo, Exception> traceFunc,
            List<VectorEvent> eventList)
            : this(listName, startState, traceFunc, eventList.ToArray())
        {

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

        public void GetEventList(Func<StateVectorTraceInfo, Exception> onceTraceFunc = FUNC_NOT_SET)
        {
            foreach (var veb in EventList)
            {
                var traceInfo = SetTraceInfo(veb.Head, veb.Tail, veb);

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

        public List<StateVectorTraceInfo> Refresh(string stateNext, Func<StateVectorTraceInfo, Exception> onceTraceFunc = FUNC_NOT_SET)
        {
            List<VectorEventBase> list = GetNextEventList(stateNext);
            List<StateVectorTraceInfo> result = new List<StateVectorTraceInfo>();

            foreach (var veb in list)
            {
                var traceInfo = SetTraceInfo(StateNow, stateNext, veb);
                result.Add(traceInfo);

                traceInfo.IsHit = true;

                if (EnableRefreshTrace)
                {
                    Trace(TraceFunc, traceInfo);//start
                }

                Trace(onceTraceFunc, traceInfo);//start

                veb.Func();

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
                result.Add(traceInfo);

                if (EnableRefreshTrace)
                {
                    Trace(TraceFunc, traceInfo);//end
                }

                Trace(onceTraceFunc, traceInfo);
            }

            StateOld = StateNow;
            StateNow = stateNext;

            return result;
        }

        protected StateVectorTraceInfo SetTraceInfo(string stateNow, string stateNext, VectorEventBase veb = null)
        {
            StateVectorTraceInfo traceInfo = new StateVectorTraceInfo
            {
                IsHit = false,
                ListName = ListName,
                Head = stateNow,
                Tail = stateNext
            };

            if (veb != null)
            {
                traceInfo.Tag = veb.Tag;
                traceInfo.Index = veb.Index;
                traceInfo.Priority = veb.Priority;
                traceInfo.FuncInfo = veb.Func.Method;
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
                .Where(veb => (veb.Head == stateNow && veb.Tail == stateNext))
                .ToList();
        }

        protected List<VectorEventBase> GetRegexp(string stateNow, string stateNext)
        {
            return EventList
                .Where(veb => (
                    veb.Head.IsMatch(stateNow) && veb.Tail.IsMatch(stateNext)))
                .ToList();
        }
    }
}
