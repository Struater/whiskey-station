// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Whiskey;

[TestFixture]
public sealed class InsurgentFactionTest : GameTest
{
    private static readonly ProtoId<NpcFactionPrototype> Insurgent = "Insurgent";
    private const string AntiInsurgent = "InsurgentDestroyer9000";

    [Test]
    public async Task ApenasTorretaAntiInsurgenteAtacaAutomaticamente()
    {
        await Server.WaitAssertion(() =>
        {
            var hostis = Server.ProtoMan.EnumeratePrototypes<NpcFactionPrototype>()
                .Where(faction => faction.Hostile.Contains(Insurgent))
                .Select(faction => faction.ID)
                .ToArray();

            Assert.That(hostis, Is.EquivalentTo(new[] { AntiInsurgent }));
        });
    }
}
