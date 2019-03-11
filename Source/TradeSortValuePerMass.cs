using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack
{
	public class TransferableComparer_ValuePerMass : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return (lhs.AnyThing.MarketValue / lhs.AnyThing.GetStatValue(StatDefOf.Mass))
				.CompareTo(rhs.AnyThing.MarketValue / rhs.AnyThing.GetStatValue(StatDefOf.Mass));
		}
	}
}
