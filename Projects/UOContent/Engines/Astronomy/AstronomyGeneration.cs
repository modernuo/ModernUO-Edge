// Source: ServUO Scripts/Services/Astronomy/Willebrord.cs (Initialize placement)
using Server.Mobiles;

namespace Server.Engines.Astronomy
{
    public static class AstronomyGeneration
    {
        public static void Configure()
        {
            CommandSystem.Register("GenAstronomy", AccessLevel.Administrator, Generate);
            CommandSystem.Register("DelAstronomy", AccessLevel.Administrator, Delete);
        }

        [Usage("GenAstronomy")]
        [Description("Enables the Astronomy system (opt-in, persisted) and places the fixed world objects (Willebrord, tent, telescope, orrery, ledger, primer) if not already present.")]
        private static void Generate(CommandEventArgs e)
        {
            // Like the Factions generator, running this opts the system in (and persists the setting).
            AstronomySystem.Enable();

            var placed = PlaceAll();
            e.Mobile.SendMessage($"Astronomy: enabled; placed {placed} missing object(s).");
        }

        /// <summary>
        /// Places all fixed Astronomy world objects that are not already present.
        /// Returns the count of newly placed objects (0 if all are already present).
        /// </summary>
        public static int PlaceAll()
        {
            var placed = 0;
            var map = Map.Trammel;

            // Willebrord NPC at (4706, 1128, 6)
            var willebrordPoint = new Point3D(4706, 1128, 6);
            var willebrordFound = false;
            foreach (var m in map.GetMobilesAt<Willebrord>(willebrordPoint))
            {
                if (m.Z == willebrordPoint.Z)
                {
                    willebrordFound = true;
                    break;
                }
            }

            if (!willebrordFound)
            {
                var npc = new Willebrord { CantWalk = true };
                npc.MoveToWorld(willebrordPoint, map);
                placed++;
            }

            // AstronomyTent at (4707, 1127, 0)
            var tentPoint = new Point3D(4707, 1127, 0);
            var tentFound = false;
            foreach (var item in map.GetItemsAt<Server.Items.AstronomyTent>(tentPoint))
            {
                if (item.Z == tentPoint.Z)
                {
                    tentFound = true;
                    break;
                }
            }

            if (!tentFound)
            {
                var tent = new Server.Items.AstronomyTent();
                tent.MoveToWorld(tentPoint, map);
                placed++;
            }

            // PersonalTelescope at (4705, 1128, 0) — Movable = false
            var telePoint = new Point3D(4705, 1128, 0);
            var teleFound = false;
            foreach (var item in map.GetItemsAt<Server.Items.PersonalTelescope>(telePoint.X, telePoint.Y))
            {
                if (item.Z == telePoint.Z)
                {
                    teleFound = true;
                    break;
                }
            }

            if (!teleFound)
            {
                var tele = new Server.Items.PersonalTelescope { Movable = false };
                tele.MoveToWorld(telePoint, map);
                placed++;
            }

            // BrassOrrery at (4705, 1126, 0) — Movable = false
            var orreryPoint = new Point3D(4705, 1126, 0);
            var orreryFound = false;
            foreach (var item in map.GetItemsAt<Server.Items.BrassOrrery>(orreryPoint.X, orreryPoint.Y))
            {
                if (item.Z == orreryPoint.Z)
                {
                    orreryFound = true;
                    break;
                }
            }

            if (!orreryFound)
            {
                var orrery = new Server.Items.BrassOrrery { Movable = false };
                orrery.MoveToWorld(orreryPoint, map);
                placed++;
            }

            // ConstellationLedger at (4709, 1127, 4) — Movable = false
            // Note: ServUO's existence check used z=0 but placed at z=4; using placement coords for both
            var ledgerPoint = new Point3D(4709, 1127, 4);
            var ledgerFound = false;
            foreach (var item in map.GetItemsAt<ConstellationLedger>(ledgerPoint.X, ledgerPoint.Y))
            {
                if (item.Z == ledgerPoint.Z)
                {
                    ledgerFound = true;
                    break;
                }
            }

            if (!ledgerFound)
            {
                var ledger = new ConstellationLedger { Movable = false };
                ledger.MoveToWorld(ledgerPoint, map);
                placed++;
            }

            // PrimerOnBritannianAstronomy at (4709, 1126, 4) — Movable = false
            // Note: ServUO's existence check used z=0 but placed at z=4; using placement coords for both
            var primerPoint = new Point3D(4709, 1126, 4);
            var primerFound = false;
            foreach (var item in map.GetItemsAt<Server.Items.PrimerOnBritannianAstronomy>(primerPoint.X, primerPoint.Y))
            {
                if (item.Z == primerPoint.Z)
                {
                    primerFound = true;
                    break;
                }
            }

            if (!primerFound)
            {
                var primer = new Server.Items.PrimerOnBritannianAstronomy { Movable = false };
                primer.MoveToWorld(primerPoint, map);
                placed++;
            }

            return placed;
        }

        [Usage("DelAstronomy")]
        [Description("Removes all fixed Astronomy world objects (Willebrord, tent, telescope, orrery, ledger, primer).")]
        private static void Delete(CommandEventArgs e)
        {
            var deleted = 0;
            var map = Map.Trammel;

            // Willebrord NPC at (4706, 1128, 6)
            var willebrordPoint = new Point3D(4706, 1128, 6);
            foreach (var m in map.GetMobilesAt<Willebrord>(willebrordPoint))
            {
                if (m.Z == willebrordPoint.Z)
                {
                    m.Delete();
                    deleted++;
                    break;
                }
            }

            // AstronomyTent at (4707, 1127, 0)
            var tentPoint = new Point3D(4707, 1127, 0);
            foreach (var item in map.GetItemsAt<Server.Items.AstronomyTent>(tentPoint))
            {
                if (item.Z == tentPoint.Z)
                {
                    item.Delete();
                    deleted++;
                    break;
                }
            }

            // PersonalTelescope at (4705, 1128, 0)
            var telePoint = new Point3D(4705, 1128, 0);
            foreach (var item in map.GetItemsAt<Server.Items.PersonalTelescope>(telePoint.X, telePoint.Y))
            {
                if (item.Z == telePoint.Z)
                {
                    item.Delete();
                    deleted++;
                    break;
                }
            }

            // BrassOrrery at (4705, 1126, 0)
            var orreryPoint = new Point3D(4705, 1126, 0);
            foreach (var item in map.GetItemsAt<Server.Items.BrassOrrery>(orreryPoint.X, orreryPoint.Y))
            {
                if (item.Z == orreryPoint.Z)
                {
                    item.Delete();
                    deleted++;
                    break;
                }
            }

            // ConstellationLedger at (4709, 1127, 4)
            var ledgerPoint = new Point3D(4709, 1127, 4);
            foreach (var item in map.GetItemsAt<ConstellationLedger>(ledgerPoint.X, ledgerPoint.Y))
            {
                if (item.Z == ledgerPoint.Z)
                {
                    item.Delete();
                    deleted++;
                    break;
                }
            }

            // PrimerOnBritannianAstronomy at (4709, 1126, 4)
            var primerPoint = new Point3D(4709, 1126, 4);
            foreach (var item in map.GetItemsAt<Server.Items.PrimerOnBritannianAstronomy>(primerPoint.X, primerPoint.Y))
            {
                if (item.Z == primerPoint.Z)
                {
                    item.Delete();
                    deleted++;
                    break;
                }
            }

            e.Mobile.SendMessage($"Astronomy: deleted {deleted} object(s).");
        }
    }
}
