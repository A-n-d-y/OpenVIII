﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Junction
        {
            #region Classes

            private class IGMData_Mag_EL_A_Values : IGMData_Values
            {

                #region Constructors

                public IGMData_Mag_EL_A_Values() : base(8, 5, new IGMDataItem_Box(title: Icons.ID.Elemental_Attack, pos: new Rectangle(280, 423, 545, 201)), 2, 4)
                {
                }

                #endregion Constructors

                #region Methods

                public Dictionary<Kernel_bin.Element, byte> getTotal(Saves.CharacterData source, out Enum[] availableFlagsarray)
                    => getTotal<Kernel_bin.Element>(out availableFlagsarray, 200, Kernel_bin.Stat.EL_Atk, source.Stat_J[Kernel_bin.Stat.EL_Atk]);

                public override bool Update()
                {
                    if (Memory.State.Characters != null)
                    {
                        Dictionary<Kernel_bin.Element, byte> oldtotal = (prevSetting != null) ? getTotal(prevSetting, out Enum[] availableFlagsarray) : null;
                        Dictionary<Kernel_bin.Element, byte> total = getTotal(Memory.State.Characters[Character], out availableFlagsarray);
                        FillData(oldtotal, total, availableFlagsarray, Icons.ID.Element_Fire, palette: 9);
                    }
                    return base.Update();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-25, -25);
                    SIZE[i].Y -= 6 * row;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}