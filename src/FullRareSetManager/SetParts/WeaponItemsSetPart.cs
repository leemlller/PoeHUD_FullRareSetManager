﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullRareSetManager
{
    public class WeaponItemsSetPart : BaseSetPart
    {
        public WeaponItemsSetPart(string partName) : base(partName) { }

        public List<StashItem> TwoHanded_HighLvlItems = new List<StashItem>();
        public List<StashItem> TwoHanded_LowLvlItems = new List<StashItem>();

        public List<StashItem> OneHanded_HighLvlItems = new List<StashItem>();
        public List<StashItem> OneHanded_LowLvlItems = new List<StashItem>();



        public override void AddItem(StashItem item)
        {
            if (item.ItemType == StashItemType.TwoHanded)
            {
                if (item.LowLvl)
                    TwoHanded_LowLvlItems.Add(item);
                else
                    TwoHanded_HighLvlItems.Add(item);
            }
            else
            {
                if (item.LowLvl)
                    OneHanded_LowLvlItems.Add(item);
                else
                    OneHanded_HighLvlItems.Add(item);
            }
        }

        public override int LowSetsCount()
        {
            int count = TwoHanded_LowLvlItems.Count;

            int oneHandedCount = 0;

            int oneHandedLeft = OneHanded_LowLvlItems.Count - OneHanded_HighLvlItems.Count;//High and low together

            if (oneHandedLeft <= 0)
                oneHandedCount = OneHanded_LowLvlItems.Count;
            else
                oneHandedCount = OneHanded_HighLvlItems.Count + oneHandedLeft / 2;//(High & low) + (Low / 2)


            return count + oneHandedCount;
        }
        public override int HighSetsCount()
        {
            return TwoHanded_HighLvlItems.Count + OneHanded_HighLvlItems.Count;
        }
        public override int TotalSetsCount()
        {
            return TwoHanded_LowLvlItems.Count + TwoHanded_HighLvlItems.Count + (OneHanded_HighLvlItems.Count + OneHanded_LowLvlItems.Count) / 2;
        }

        public override string GetInfoString()
        {
            string rezult = "Weapons: " + TotalSetsCount() + " (" + LowSetsCount() + "L / " + HighSetsCount() + "H)";

            var TwoHandCount = TwoHanded_LowLvlItems.Count + TwoHanded_HighLvlItems.Count;
            if (TwoHandCount > 0)
                rezult += "\r\n     Two Handed: " + TwoHandCount + " (" + TwoHanded_LowLvlItems.Count + "L / " + TwoHanded_HighLvlItems.Count + "H)";

            var OneHandCount = OneHanded_LowLvlItems.Count + OneHanded_HighLvlItems.Count;
            if (OneHandCount > 0)
                rezult += "\r\n     One Handed: " + OneHandCount / 2 + " (" + OneHanded_LowLvlItems.Count + "L / " + OneHanded_HighLvlItems.Count + "H)";

            return rezult;
        }


        private StashItem[] CurrentSetItems;
        public override PrepareItemResult PrepareItemForSet(FullRareSetManager_Settings settings)
        {
            bool oneHandedFirst = settings.WeaponTypePriority == 1;

            if (!oneHandedFirst)
            {
                if (OneHanded_HighLvlItems.Count > 0 && OneHanded_HighLvlItems[0].bInPlayerInventory)
                    oneHandedFirst = true;
                else if (OneHanded_LowLvlItems.Count > 0 && OneHanded_LowLvlItems[0].bInPlayerInventory)
                    oneHandedFirst = true;
            }


            Func<PrepareItemResult>[] invokeList = new Func<PrepareItemResult>[5];


            if(oneHandedFirst)
            {
                invokeList[0] = Prepahe_OH;
                invokeList[1] = Prepahe_TH;
                invokeList[2] = Prepahe_OHOL;
                invokeList[3] = Prepahe_OL;
                invokeList[4] = Prepahe_TL;
            }
            else
            {
                invokeList[0] = Prepahe_TH;
                invokeList[1] = Prepahe_OH;
                invokeList[2] = Prepahe_OHOL;
                invokeList[3] = Prepahe_TL;
                invokeList[4] = Prepahe_OL;
            }

            for (int i = 0; i < invokeList.Length; i++)
            {
                var result = invokeList[i]();

                if (result != null)
                    return result;
            }







            /*
            var result = Prepahe_TH();//Two handed high

            if (result == null)
            {
                result = Prepahe_OH();//One handed high

                if (result == null)
                {
                    result = Prepahe_OHOL();//One handed high + low

                    if (result == null)
                    {
                        result = Prepahe_TL();//Two handed low

                        if (result == null)
                        {
                            result = Prepahe_OL();//One handed low
                        }
                    }
                }
            }
            */
            return null;
        }

        private PrepareItemResult Prepahe_TH()
        {
            if (TwoHanded_HighLvlItems.Count >= 1)
            {
                CurrentSetItems = new StashItem[]
                {
                    TwoHanded_HighLvlItems[0]
                };

                return new PrepareItemResult() { AllowedReplacesCount = LowSetsCount(), LowSet = false, bInPlayerInvent = CurrentSetItems[0].bInPlayerInventory };
            }
            return null;
        }

        private PrepareItemResult Prepahe_OH()
        {
            if (OneHanded_HighLvlItems.Count >= 2)
            {
                CurrentSetItems = new StashItem[]
                {
                    OneHanded_HighLvlItems[0],
                    OneHanded_HighLvlItems[1]
                };
                var inPlayerInvent = CurrentSetItems[0].bInPlayerInventory || CurrentSetItems[1].bInPlayerInventory;
                return new PrepareItemResult() { AllowedReplacesCount = LowSetsCount(), LowSet = false, bInPlayerInvent = inPlayerInvent };
            }
            return null;
        }

        private PrepareItemResult Prepahe_OHOL()
        {
            if (OneHanded_HighLvlItems.Count >= 1 && OneHanded_LowLvlItems.Count >= 1)
            {
                CurrentSetItems = new StashItem[]
                {
                    OneHanded_HighLvlItems[0],
                    OneHanded_LowLvlItems[1]
                };

                int replCount = TwoHanded_LowLvlItems.Count;
                int oneHandedLowCount = OneHanded_LowLvlItems.Count - 1;

                int oneHandedCount = 0;

                int oneHandedLeft = oneHandedLowCount - OneHanded_HighLvlItems.Count;//High and low together

                if (oneHandedLeft <= 0)
                    oneHandedCount = oneHandedLowCount;
                else
                    oneHandedCount = OneHanded_HighLvlItems.Count + oneHandedLeft / 2;//(High & low) + (Low / 2)

                replCount += oneHandedCount;

                var inPlayerInvent = CurrentSetItems[0].bInPlayerInventory || CurrentSetItems[1].bInPlayerInventory;
                return new PrepareItemResult() { AllowedReplacesCount = replCount, LowSet = true, bInPlayerInvent = inPlayerInvent };
            }
            return null;
        }

        private PrepareItemResult Prepahe_TL()
        {
            if (TwoHanded_LowLvlItems.Count >= 1)
            {
                CurrentSetItems = new StashItem[]
                {
                    TwoHanded_LowLvlItems[0]
                };

                var replCount = LowSetsCount() - 1;
                return new PrepareItemResult() { AllowedReplacesCount = replCount, LowSet = true, bInPlayerInvent = CurrentSetItems[0].bInPlayerInventory };
            }
            return null;
        }
        private PrepareItemResult Prepahe_OL()
        {
            if (OneHanded_LowLvlItems.Count >= 2)
            {
                CurrentSetItems = new StashItem[]
                {
                    OneHanded_LowLvlItems[0],
                    OneHanded_LowLvlItems[1]
                };

                var replCount = LowSetsCount() - 2;
                var inPlayerInvent = CurrentSetItems[0].bInPlayerInventory || CurrentSetItems[1].bInPlayerInventory;
                return new PrepareItemResult() { AllowedReplacesCount = replCount, LowSet = true, bInPlayerInvent = inPlayerInvent };
            }
            return null;
        }

        public override void DoLowItemReplace()
        {
            if (TwoHanded_LowLvlItems.Count >= 1)
            {
                CurrentSetItems = new StashItem[]
                {
                    TwoHanded_LowLvlItems[0]
                };
            }
            else if (OneHanded_HighLvlItems.Count >= 1 && OneHanded_LowLvlItems.Count >= 1)
            {
                CurrentSetItems = new StashItem[]
                {
                    OneHanded_HighLvlItems[0],
                    OneHanded_LowLvlItems[0]
                };
            }
            else if (OneHanded_LowLvlItems.Count >= 2)
            {
                CurrentSetItems = new StashItem[]
                {
                    OneHanded_LowLvlItems[0],
                    OneHanded_LowLvlItems[1]
                };
            }
            else
            {
                PoeHUD.DebugPlug.DebugPlugin.LogMsg("Something goes wrong: Can't do low lvl item replace on weapons!", 10, SharpDX.Color.Red);
            }
        }

        public override StashItem[] GetPreparedItems()
        {
            return CurrentSetItems;
        }
    }
}