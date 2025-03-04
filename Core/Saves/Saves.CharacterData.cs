﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static partial class Saves
    {
        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public class CharacterData
        {
            public FF8String Name; //not saved to file.
            private ushort _CurrentHP; //0x00 -- forgot this one heh

            /// <summary>
            /// Raw HP buff from items.
            /// </summary>
            public ushort _HP; //0x02

            public uint Experience; //0x02
            public byte ModelID; //0x04
            public byte WeaponID; //0x08

            /// <summary>
            /// Stats that can be incrased via items. Except for HP because it's a ushort not a byte.
            /// </summary>
            public Dictionary<Kernel_bin.Stat, byte> RawStats;

            //public byte _STR; //0x09
            //public byte _VIT; //0x0A
            //public byte _MAG; //0x0B
            //public byte _SPR; //0x0C
            //public byte _SPD; //0x0D
            //public byte _LCK; //0x0E
            public Dictionary<byte, byte> Magics; //0x0F

            public List<Kernel_bin.Abilities> Commands; //0x10
            public byte Paddingorunusedcommand; //0x50
            public List<Kernel_bin.Abilities> Abilities; //0x53
            public GFflags JunctionnedGFs; //0x54
            public byte Unknown1; //0x58
            public byte Alternativemodel; //0x5A (Normal, SeeD, Soldier...)
            public Dictionary<Kernel_bin.Stat, byte> Stat_J;

            //public byte JunctionHP; //0x5B
            //public byte JunctionSTR; //0x5C
            //public byte JunctionVIT; //0x5D
            //public byte JunctionMAG; //0x5E
            //public byte JunctionSPR; //0x5F
            //public byte JunctionSPD; //0x60
            //public byte JunctionEVA; //0x61
            //public byte JunctionHIT; //0x62
            //public byte JunctionLCK; //0x63
            //public byte Elem_Atk_J; //0x64
            //public byte ST_Atk_J; //0x65
            //public List<byte> Elem_Def_J; //0x66
            //public List<byte> ST_Def_J; //0x67
            public byte Unknown2; //0x6B (padding?)

            public Dictionary<GFs, ushort> CompatibilitywithGFs; //0x6F
            public ushort Numberofkills; //0x70
            public ushort NumberofKOs; //0x90
            public byte Exists; //0x92 //15,9,7,4,1 shows on menu, 0 locked, 6 hidden // I think I wonder if this is a flags value.
            public bool VisibleInMenu => Exists != 0 && Exists != 6;
            public bool CanAddToParty => true; // I'm sure one of the Exists values determines this but I donno yet.
            public byte Unknown3; //0x94
            public byte MentalStatus; //0x95
            public byte Unknown4; //0x96

            public CharacterData()
            {
            }

            public CharacterData(BinaryReader br, Characters c) => Read(br, c);

            public Characters ID { get; private set; }
            public List<Kernel_bin.Abilities> UnlockedGFAbilities
            {
                get
                {
                    BitArray total = new BitArray(16 * 8);
                    List<Kernel_bin.Abilities> abilities = new List<Kernel_bin.Abilities>();
                    IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(GFflags)).Cast<Enum>();
                    foreach (Enum flag in availableFlags.Where(JunctionnedGFs.HasFlag))
                    {
                        if ((GFflags)flag == GFflags.None) continue;
                        total.Or(Memory.State.GFs[ConvertGFEnum[(GFflags)flag]].Complete);
                    }
                    for (int i = 1; i < total.Length; i++)//0 is none so skipping it.
                    {
                        if (total[i])
                            abilities.Add((Kernel_bin.Abilities)i);
                    }

                    return abilities;
                }
            }

            /// <summary>
            /// Sorted Enumerable based on best to worst for Stat. Uses character's total magic and
            /// kernel bin's stat value.
            /// </summary>
            /// <param name="Stat">Stat sorting by.</param>
            /// <returns>Ordered Enumberable</returns>
            public IOrderedEnumerable<Kernel_bin.Magic_Data> SortedMagic(Kernel_bin.Stat Stat) => Kernel_bin.MagicData.OrderBy(x => (-x.totalStatVal(Stat) * (Magics.ContainsKey(x.ID) ? Magics[x.ID] : 0)) / 100);

            public void RemoveMagic() => Stat_J = Stat_J.ToDictionary(e => e.Key, e => (byte)0);

            public void RemoveAll()
            {
                Stat_J = Stat_J.ToDictionary(e => e.Key, e => (byte)0);
                Commands = Commands.ConvertAll(Item => Kernel_bin.Abilities.None);
                Abilities = Abilities.ConvertAll(Item => Kernel_bin.Abilities.None);
                JunctionnedGFs = GFflags.None;
            }

            public void Read(BinaryReader br, Characters c)
            {
                ID = c;
                _CurrentHP = br.ReadUInt16();//0x00
                _HP = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                RawStats = new Dictionary<Kernel_bin.Stat, byte>(6)
                {
                    [Kernel_bin.Stat.STR] = br.ReadByte(),//0x0A
                    [Kernel_bin.Stat.VIT] = br.ReadByte(),//0x0B
                    [Kernel_bin.Stat.MAG] = br.ReadByte(),//0x0C
                    [Kernel_bin.Stat.SPR] = br.ReadByte(),//0x0D
                    [Kernel_bin.Stat.SPD] = br.ReadByte(),//0x0E
                    [Kernel_bin.Stat.LUCK] = br.ReadByte()//0x0F
                };
                Magics = new Dictionary<byte, byte>(33);
                for (int i = 0; i < 32; i++)
                {
                    byte key = br.ReadByte();
                    byte val = br.ReadByte();
                    if (key >= 0 && !Magics.ContainsKey(key))
                        Magics.Add(key, val);//0x10
                }
                Commands = Array.ConvertAll(br.ReadBytes(3), Item => (Kernel_bin.Abilities)Item).ToList();//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = Array.ConvertAll(br.ReadBytes(4), Item => (Kernel_bin.Abilities)Item).ToList();//0x54
                JunctionnedGFs = (GFflags)br.ReadUInt16();//0x58
                Unknown1 = br.ReadByte();//0x5A
                Alternativemodel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                Stat_J = new Dictionary<Kernel_bin.Stat, byte>(9);
                for (int i = 0; i < 19; i++)
                {
                    Kernel_bin.Stat key = (Kernel_bin.Stat)i;
                    byte val = br.ReadByte();
                    if (!Stat_J.ContainsKey(key))
                        Stat_J.Add(key, val);
                }
                //JunctionHP = br.ReadByte();//0x5C
                //JunctionSTR = br.ReadByte();//0x5D
                //JunctionVIT = br.ReadByte();//0x5E
                //JunctionMAG = br.ReadByte();//0x5F
                //JunctionSPR = br.ReadByte();//0x60
                //JunctionSPD = br.ReadByte();//0x61
                //JunctionEVA = br.ReadByte();//0x62
                //JunctionHIT = br.ReadByte();//0x63
                //JunctionLCK = br.ReadByte();//0x64
                //Elem_Atk_J = br.ReadByte();//0x65
                //ST_Atk_J = br.ReadByte();//0x66
                //Elem_Def_J = br.ReadBytes(4).ToList() ;//0x67
                //ST_Def_J = br.ReadBytes(4).ToList();//0x6B
                Unknown2 = br.ReadByte();//0x6F (padding?)
                CompatibilitywithGFs = new Dictionary<GFs, ushort>(16);
                for (int i = 0; i < 16; i++)
                    CompatibilitywithGFs.Add((GFs)i, br.ReadUInt16());//0x70
                Numberofkills = br.ReadUInt16();//0x90
                NumberofKOs = br.ReadUInt16();//0x92
                Exists = br.ReadByte();//0x94
                Unknown3 = br.ReadByte();//0x95
                MentalStatus = br.ReadByte();//0x96
                Unknown4 = br.ReadByte();//0x97
            }

            public void AutoMAG() => Auto(Kernel_bin.AutoMAG);

            public void AutoDEF() => Auto(Kernel_bin.AutoDEF);

            public void AutoATK() => Auto(Kernel_bin.AutoATK);

            private void Auto(IReadOnlyList<Kernel_bin.Stat> list)
            {
                RemoveMagic();

                List<Kernel_bin.Abilities> unlockedlist = UnlockedGFAbilities;
                foreach (Kernel_bin.Stat stat in list)
                {
                    if (Unlocked(unlockedlist, stat))
                        foreach (Kernel_bin.Magic_Data spell in SortedMagic(stat))
                        {
                            if (!Stat_J.ContainsValue(spell.ID))
                            {
                                //TODO make smarter.
                                //example if you can get max stat with a weaker spell use that first.

                                // if stat is max with out spell skip
                                if (stat != Kernel_bin.Stat.HP && TotalStat(stat) == Kernel_bin.Character_Stats.MAX_STAT_VALUE) break;
                                // if hp is max without spell skip
                                else if (stat == Kernel_bin.Stat.HP && TotalStat(stat) == Kernel_bin.Character_Stats.MAX_HP_VALUE) break;
                                // junction spell
                                else Stat_J[stat] = spell.ID;
                                break;
                            }
                        }
                }
            }

            public bool Unlocked(Kernel_bin.Stat stat) => Unlocked(UnlockedGFAbilities, stat);

            public bool Unlocked(List<Kernel_bin.Abilities> unlocked, Kernel_bin.Stat stat)
            {
                switch (stat)
                {
                    default:
                        return unlocked.Contains(Kernel_bin.Stat2Ability[stat]);

                    case Kernel_bin.Stat.EL_Atk:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Atk_J);

                    case Kernel_bin.Stat.EL_Def_1:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx1) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.EL_Def_2:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.EL_Def_3:
                    case Kernel_bin.Stat.EL_Def_4:
                        return unlocked.Contains(Kernel_bin.Abilities.EL_Def_Jx4);

                    case Kernel_bin.Stat.ST_Atk:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Atk_J);

                    case Kernel_bin.Stat.ST_Def_1:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx1) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);

                    case Kernel_bin.Stat.ST_Def_2:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx2) ||
                            unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);

                    case Kernel_bin.Stat.ST_Def_3:
                    case Kernel_bin.Stat.ST_Def_4:
                        return unlocked.Contains(Kernel_bin.Abilities.ST_Def_Jx4);
                }
            }

            public int Level => (int)((Experience / 1000) + 1);
            public int ExperienceToNextLevel => (int)((Level) * 1000 - Experience);

            /// <summary>
            /// Max HP
            /// </summary>
            /// <param name="c">Force another character's HP calculation</param>
            /// <returns></returns>
            public ushort MaxHP(Characters c = Characters.Blank) => TotalStat(Kernel_bin.Stat.HP, c);

            public ushort TotalStat(Kernel_bin.Stat s, Characters c = Characters.Blank)
            {
                if (c == Characters.Blank)
                    c = ID;
                int total = 0;
                foreach (Kernel_bin.Abilities i in Abilities)
                {
                    if (Kernel_bin.Statpercentabilities.ContainsKey(i) && Kernel_bin.Statpercentabilities[i].Stat == s)
                        total += Kernel_bin.Statpercentabilities[i].Value;
                }
                switch (s)
                {
                    case Kernel_bin.Stat.HP:
                        return Kernel_bin.CharacterStats[c].HP((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], _HP, total);

                    case Kernel_bin.Stat.EVA:
                        //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                        return Kernel_bin.CharacterStats[c].EVA((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], 0, TotalStat(Kernel_bin.Stat.SPD, c), total);

                    case Kernel_bin.Stat.SPD:
                        return Kernel_bin.CharacterStats[c].SPD((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.HIT:
                        return Kernel_bin.CharacterStats[c].HIT(Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], WeaponID);

                    case Kernel_bin.Stat.LUCK:
                        return Kernel_bin.CharacterStats[c].LUCK((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.MAG:
                        return Kernel_bin.CharacterStats[c].MAG((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.SPR:
                        return Kernel_bin.CharacterStats[c].SPR((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);

                    case Kernel_bin.Stat.STR:
                        return Kernel_bin.CharacterStats[c].STR((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total, WeaponID);

                    case Kernel_bin.Stat.VIT:
                        return Kernel_bin.CharacterStats[c].VIT((sbyte)Level, Stat_J[s], Stat_J[s] == 0 ? 0 : Magics[Stat_J[s]], RawStats[s], total);
                }
                return 0;
            }

            public ushort CurrentHP(Characters c = Characters.Blank)
            {
                ushort max = MaxHP(c);
                if (max < _CurrentHP) _CurrentHP = max;
                return _CurrentHP;
            }

            public void JunctionSpell(Kernel_bin.Stat stat, byte spell)
            {
                //see if magic is in use, if so remove it
                if (Stat_J.ContainsValue(spell))
                {
                    Kernel_bin.Stat key = Stat_J.FirstOrDefault(x => x.Value == spell).Key;
                    Stat_J[key] = 0;
                }
                //junction magic
                Stat_J[stat] = spell;
            }

            public float PercentFullHP(Characters c = Characters.Blank) => (float)_CurrentHP / MaxHP(c);

            public override string ToString() => Name.Length > 0 ? Name.ToString() : base.ToString();

            public CharacterData Clone()
            {
                //Shadowcopy
                CharacterData c = (CharacterData)MemberwiseClone();
                //Deepcopy
                c.CompatibilitywithGFs = CompatibilitywithGFs.ToDictionary(e => e.Key, e => e.Value);
                c.Stat_J = Stat_J.ToDictionary(e => e.Key, e => e.Value);
                c.Magics = Magics.ToDictionary(e => e.Key, e => e.Value);
                c.RawStats = RawStats.ToDictionary(e => e.Key, e => e.Value);
                c.Commands = Commands.ConvertAll(Item => Item);
                c.Abilities = Abilities.ConvertAll(Item => Item);
                return c;
            }
        }
    }
}