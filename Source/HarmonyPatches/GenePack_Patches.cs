using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalBiotech
{
    [HarmonyPatch(typeof(CompGenepackContainer), "PowerOn", MethodType.Getter)]
    public static class CompGenepackContainer_PowerOn_Patch
    {
        public static bool Prefix(ref bool __result, CompGenepackContainer __instance)
        {
            __result = __instance.parent.TryGetComp<CompRefuelable>().HasFuel;
            return false;
        }
    }



    [HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
    public static class Pawn_TraderTracker_ColonyThingsWillingToBuy_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn_TraderTracker __instance)
        {
            foreach (Thing thing in __result)
            {
                yield return thing;
            }
            if (ModsConfig.BiotechActive)
            {
                IEnumerable<Building> enumerable2 = __instance.pawn.Map.listerBuildings.AllBuildingsColonistOfDef(MB_DefOf.MB_GenePotionsRack);
                foreach (Building item2 in enumerable2)
                {
                    if (!__instance.ReachableForTrade(item2))
                    {
                        continue;
                    }
                    CompGenepackContainer compGenepackContainer = item2.TryGetComp<CompGenepackContainer>();
                    if (compGenepackContainer == null)
                    {
                        continue;
                    }
                    List<Genepack> containedGenepacks = compGenepackContainer.ContainedGenepacks;
                    foreach (Genepack item3 in containedGenepacks)
                    {
                        yield return item3;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TradeUtility), "AllLaunchableThingsForTrade")]
    public static class TradeUtility_AllLaunchableThingsForTrade_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Map map, ITrader trader = null)
        {
            foreach (Thing thing in __result)
            {
                yield return thing;
            }
            HashSet<Thing> yieldedThings = new HashSet<Thing>();
            foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
            {
                foreach (IntVec3 tradeableCell in item.TradeableCells)
                {
                    List<Thing> thingList = tradeableCell.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing t = thingList[i];
                        if (ModsConfig.BiotechActive && t.def == MB_DefOf.MB_GenePotionsRack)
                        {
                            CompGenepackContainer compGenepackContainer = t.TryGetComp<CompGenepackContainer>();
                            if (compGenepackContainer == null)
                            {
                                continue;
                            }
                            List<Genepack> containedGenepacks = compGenepackContainer.ContainedGenepacks;
                            foreach (Genepack item2 in containedGenepacks)
                            {
                                if (TradeUtility.PlayerSellableNow(t, trader) && !yieldedThings.Contains(item2))
                                {
                                    yieldedThings.Add(item2);
                                    yield return item2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
