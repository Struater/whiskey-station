// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Whiskey;

// Whiskey - regression coverage for the Blood Bolt Barrage hand and range fixes.
public sealed class BloodBoltBarrageTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    [Test]
    public async Task BarrageKeepsHandAndHitsAtRange()
    {
        await AddAtmosphere();

        TargetCoords = SEntMan.GetNetCoordinates(
            new EntityCoordinates(MapData.MapUid, new Vector2(6.5f, 0.5f)));
        var target = await SpawnTarget("MobHuman", TargetCoords);
        var barrage = await PlaceInHands("BloodBoltBarrage");

        await Pair.RunSeconds(2f); // Guns start with a pickup cooldown.

        var activeHand = HandSys.GetActiveHand((SPlayer, Hands));
        Assert.That(activeHand, Is.Not.Null);

        await AttemptShoot(target);
        await Pair.RunSeconds(0.5f);

        Assert.That(HandSys.GetActiveHand((SPlayer, Hands)), Is.EqualTo(activeHand));
        Assert.That(HandSys.GetActiveItem((SPlayer, Hands)), Is.EqualTo(ToServer(barrage)));

        var damageable = SEntMan.System<DamageableSystem>();
        var slash = damageable.GetAllDamage(ToServer(target)).DamageDict["Slash"];
        Assert.That(slash, Is.GreaterThan(FixedPoint2.Zero));
    }
}
