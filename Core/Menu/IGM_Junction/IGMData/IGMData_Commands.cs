﻿using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Commands : IGMData
            {
                public IGMData_Commands() : base( 4, 1, new IGMDataItem_Box(pos: new Rectangle(615, 150, 210, 192), title: Icons.ID.COMMAND), 1, 4)
                {
                }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    base.ReInit();

                    if (Memory.State.Characters != null)
                    {
                        ITEM[0, 0] = new IGMDataItem_String(
                                Kernel_bin.BattleCommands[
                                    Memory.State.Characters[Character].Abilities.Contains(Kernel_bin.Abilities.Mug) ?
                                    13 :
                                    1].Name,
                                SIZE[0]);

                        for (int pos = 1; pos < SIZE.Length; pos++)
                        {
                            ITEM[pos, 0] = Memory.State.Characters[Character].Commands[pos - 1] != Kernel_bin.Abilities.None ? new IGMDataItem_String(
                                Kernel_bin.Commandabilities[Memory.State.Characters[Character].Commands[pos - 1]].Name,
                                SIZE[pos]) : null;
                        }
                    }
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 12 + (-8 * row));
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init() => base.Init();
            }
        }
    }
}