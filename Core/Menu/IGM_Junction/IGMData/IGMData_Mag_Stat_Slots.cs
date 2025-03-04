﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {

        #region Classes

        private partial class IGM_Junction
        {

            #region Classes

            private class IGMData_Mag_Stat_Slots : IGMData_Slots<Saves.CharacterData>
            {

                #region Constructors

                public IGMData_Mag_Stat_Slots() : base(10, 5, new IGMDataItem_Box(pos: new Rectangle(0, 414, 840, 216)), 2, 5)
                {
                }

                #endregion Constructors

                #region Properties

                /// <summary>
                /// Convert stat to correct icon id.
                /// </summary>
                private static IReadOnlyDictionary<Kernel_bin.Stat, Icons.ID> Stat2Icon { get; } = new Dictionary<Kernel_bin.Stat, Icons.ID>
                {
                    { Kernel_bin.Stat.HP, Icons.ID.Stats_Hit_Points },
                    { Kernel_bin.Stat.STR, Icons.ID.Stats_Strength },
                    { Kernel_bin.Stat.VIT, Icons.ID.Stats_Vitality },
                    { Kernel_bin.Stat.MAG, Icons.ID.Stats_Magic },
                    { Kernel_bin.Stat.SPR, Icons.ID.Stats_Spirit },
                    { Kernel_bin.Stat.SPD, Icons.ID.Stats_Speed },
                    { Kernel_bin.Stat.EVA, Icons.ID.Stats_Evade },
                    { Kernel_bin.Stat.LUCK, Icons.ID.Stats_Luck },
                    { Kernel_bin.Stat.HIT, Icons.ID.Stats_Hit_Percent },
                };

                #endregion Properties

                #region Methods

                public override void BackupSetting() => SetPrevSetting(Memory.State.Characters[Character].Clone());

                public override void CheckMode(bool cursor = true) =>
                   CheckMode(-1, Mode.None, Mode.Mag_Stat,
                       InGameMenu_Junction != null && (InGameMenu_Junction.GetMode() == Mode.Mag_Stat),
                       InGameMenu_Junction != null && (InGameMenu_Junction.GetMode() == Mode.Mag_Pool_Stat),
                       (InGameMenu_Junction.GetMode() == Mode.Mag_Stat || InGameMenu_Junction.GetMode() == Mode.Mag_Pool_Stat) && cursor);

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.SetMode(Mode.TopMenu_Junction);
                    InGameMenu_Junction.Data[SectionName.Mag_Group].Hide();
                }

                public override void Inputs_Left()
                {
                    base.Inputs_Left();
                    if (CURSOR_SELECT < Count / cols || BLANKS[CURSOR_SELECT - Count / cols])
                    {
                        PageLeft();
                    }
                    else
                    {
                        CURSOR_SELECT -= Count / cols;
                    }
                }

                public override void Inputs_OKAY()
                {
                    if (!BLANKS[CURSOR_SELECT])
                    {
                        base.Inputs_OKAY();
                        BackupSetting();
                        InGameMenu_Junction.SetMode(Mode.Mag_Pool_Stat);
                    }
                }

                public override void Inputs_Right()
                {
                    base.Inputs_Right();
                    if (CURSOR_SELECT < Count / cols && !BLANKS[CURSOR_SELECT + Count / cols])
                    {
                        //if (CURSOR_SELECT == 0) CURSOR_SELECT++;
                        CURSOR_SELECT += Count / cols;
                    }
                    else
                    {
                        PageRight();
                    }
                }

                public override void Inputs_Square()
                {
                    skipdata = true;
                    base.Inputs_Square();
                    skipdata = false;
                    if (Contents[CURSOR_SELECT] == Kernel_bin.Stat.None)
                    {
                        Memory.State.Characters[Character].Stat_J[Contents[CURSOR_SELECT]] = 0;
                        InGameMenu_Junction.ReInit();
                    }
                }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    if (Memory.State.Characters != null)
                    {
                        Contents = Array.ConvertAll(Contents, c => c = default);
                        base.ReInit();

                        if (Memory.State.Characters != null)
                        {
                            ITEM[5, 0] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Attack, new Rectangle(SIZE[5].X + 200, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Atk_J) ? 2 : 7));
                            ITEM[5, 1] = new IGMDataItem_Icon(Icons.ID.Icon_Status_Defense, new Rectangle(SIZE[5].X + 240, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx1) ||
                                unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                                unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4) ? 2 : 7));
                            ITEM[5, 2] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Attack, new Rectangle(SIZE[5].X + 280, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.EL_Atk_J) ? 2 : 7));
                            ITEM[5, 3] = new IGMDataItem_Icon(Icons.ID.Icon_Elemental_Defense, new Rectangle(SIZE[5].X + 320, SIZE[5].Y, 0, 0),
                                (byte)(unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx1) ||
                                unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx2) ||
                                unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4) ? 2 : 7));
                            BLANKS[5] = true;
                            foreach (Kernel_bin.Stat stat in (Kernel_bin.Stat[])Enum.GetValues(typeof(Kernel_bin.Stat)))
                            {
                                if (Stat2Icon.ContainsKey(stat))
                                {
                                    int pos = (int)stat;
                                    if (pos >= 5) pos++;
                                    Contents[pos] = stat;
                                    FF8String name = Kernel_bin.MagicData[Memory.State.Characters[Character].Stat_J[stat]].Name;
                                    if (name.Length == 0) name = Misc[Items._];

                                    ITEM[pos, 0] = new IGMDataItem_Icon(Stat2Icon[stat], new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), 2);
                                    ITEM[pos, 1] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X + 80, SIZE[pos].Y, 0, 0));
                                    if (!unlocked.Contains(Kernel_bin.Stat2Ability[stat]))
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Palette = ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Palette = 7;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Grey;
                                        BLANKS[pos] = true;
                                    }
                                    else BLANKS[pos] = false;

                                    ITEM[pos, 2] = new IGMDataItem_Int(Memory.State.Characters[Character].TotalStat(stat, VisableCharacter), new Rectangle(SIZE[pos].X + 152, SIZE[pos].Y, 0, 0), 2, Icons.NumType.sysFntBig, spaces: 10);
                                    ITEM[pos, 3] = stat == Kernel_bin.Stat.HIT || stat == Kernel_bin.Stat.EVA
                                        ? new IGMDataItem_String(Misc[Items.Percent], new Rectangle(SIZE[pos].X + 350, SIZE[pos].Y, 0, 0))
                                        : null;
                                    if (GetPrevSetting() == null || GetPrevSetting().Stat_J[stat] == Memory.State.Characters[Character].Stat_J[stat] || GetPrevSetting().TotalStat(stat, VisableCharacter) == Memory.State.Characters[Character].TotalStat(stat, VisableCharacter))
                                    {
                                        ITEM[pos, 4] = null;
                                    }
                                    else if (GetPrevSetting().TotalStat(stat, VisableCharacter) > Memory.State.Characters[Character].TotalStat(stat, VisableCharacter))
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Palette = 5;
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Palette = 5;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Red;
                                        ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Red;
                                        if (ITEM[pos, 3] != null)
                                            ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Red;
                                        ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Down, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 16);
                                    }
                                    else
                                    {
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Palette = 6;
                                        ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Palette = 6;
                                        ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Yellow;
                                        ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Yellow;
                                        if (ITEM[pos, 3] != null)
                                            ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Yellow;
                                        ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 17);
                                    }
                                }
                            }
                        }
                    }
                }

                public override void UndoChange()
                {
                    //override this use it to take value of prevSetting and restore the setting unless default method works
                    if (GetPrevSetting() != null)
                    {
                        Memory.State.Characters[Character] = GetPrevSetting().Clone();
                    }
                }

                protected override void AddEventListener()
                {
                    if (!eventAdded)
                    {
                        IGMData_Mag_Pool.SlotConfirmListener += ConfirmChangeEvent;
                        IGMData_Mag_Pool.SlotReinitListener += ReInitEvent;
                        IGMData_Mag_Pool.SlotUndoListener += UndoChangeEvent;
                    }
                    base.AddEventListener();
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init()
                {
                    Contents = new Kernel_bin.Stat[Count];
                    base.Init();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(0, 4 + (-2 * row));
                }

                protected override void ModeChangeEvent(object sender, Mode e)
                {
                    if (e == Mode.Mag_Stat)
                        base.ModeChangeEvent(sender, e);
                }

                protected override void PageLeft() => InGameMenu_Junction.SetMode(Mode.Mag_EL_A);

                protected override void PageRight() => InGameMenu_Junction.SetMode(Mode.Mag_ST_A);

                protected override void SetCursor_select(int value)
                {
                    if (value != GetCursor_select())
                    {
                        base.SetCursor_select(value);
                        IGMData_Mag_Pool.StatEventListener?.Invoke(this, Contents[CURSOR_SELECT]);
                    }
                }

                private void ConfirmChangeEvent(object sender, Mode e) => ConfirmChange();

                private void ReInitEvent(object sender, Mode e) => ReInit();

                private void UndoChangeEvent(object sender, Mode e) => UndoChange();

                #endregion Methods

            }

            #endregion Classes

        }

        #endregion Classes

    }
}