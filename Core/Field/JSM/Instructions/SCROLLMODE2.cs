﻿using System;


namespace OpenVIII
{
    internal sealed class SCROLLMODE2 : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;
        private IJsmExpression _arg2;
        private IJsmExpression _arg3;
        private IJsmExpression _arg4;

        public SCROLLMODE2(IJsmExpression arg0, IJsmExpression arg1, IJsmExpression arg2, IJsmExpression arg3, IJsmExpression arg4)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _arg4 = arg4;
        }

        public SCROLLMODE2(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg4: stack.Pop(),
                arg3: stack.Pop(),
                arg2: stack.Pop(),
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SCROLLMODE2)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1}, {nameof(_arg2)}: {_arg2}, {nameof(_arg3)}: {_arg3}, {nameof(_arg4)}: {_arg4})";
        }
    }
}