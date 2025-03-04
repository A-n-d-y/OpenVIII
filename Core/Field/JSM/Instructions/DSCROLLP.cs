﻿using System;


namespace OpenVIII
{
    internal sealed class DSCROLLP : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DSCROLLP(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DSCROLLP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DSCROLLP)}({nameof(_arg0)}: {_arg0})";
        }
    }
}