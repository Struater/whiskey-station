// SPDX-FileCopyrightText: 2026 Zequinza <felipe828218@gmail.com>
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Server.StationEvents.Components;
using Content.Trauma.Server.Weather;
using Content.Trauma.Shared.Weather;
using NUnit.Framework;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Whiskey;

/// <summary>
/// Trava as promessas que a tempestade radioativa faz ao jogador.
///
/// O anúncio manda correr para as manutenções, e isso só é verdade enquanto
/// AreaMaints estiver na lista de áreas seguras. Um anúncio que mente é pior
/// que anúncio nenhum, porque a pessoa corre para o lugar errado e morre lá.
/// </summary>
[TestFixture]
public sealed class RadiationStormTest : GameTest
{
    [SidedDependency(Side.Server)] private readonly ILocalizationManager _loc = null!;

    private static readonly EntProtoId Evento = "RadiationStormRule";
    private static readonly EntProtoId Agendador = "WeatherSchedulerRadiationStorm";
    private static readonly EntProtoId Tempestade = "WeatherRadiationStorm";
    private static readonly EntProtoId Maints = "AreaMaints";

    /// <summary>
    /// O que o anúncio promete tem que ser verdade no clima.
    /// </summary>
    [Test]
    public async Task ManutencaoProtegeDeVerdade()
    {
        var protos = Server.ProtoMan;
        var nome = Server.EntMan.ComponentFactory.GetComponentName(typeof(WeatherEffectsComponent));

        Assert.That(protos.Index(Tempestade).Components.TryGetComponent(nome, out var raw), Is.True,
            "o clima da tempestade precisa ter WeatherEffects");

        var efeitos = (WeatherEffectsComponent) raw!;

        var seguras = efeitos.SafeAreas.Select(a => a.Id).ToList();
        TestContext.Out.WriteLine("áreas seguras: " + string.Join(", ", seguras));

        Assert.That(seguras, Does.Contain(Maints.Id),
            "o anúncio manda ir para as manutenções, então elas têm que estar na lista de áreas seguras");

        await Task.CompletedTask;
    }

    /// <summary>
    /// O evento precisa ser ouvido, não só lido. Ele entrava mudo.
    /// </summary>
    [Test]
    public async Task OEventoAnunciaEFazBarulho()
    {
        var protos = Server.ProtoMan;
        var nome = Server.EntMan.ComponentFactory.GetComponentName(typeof(StationEventComponent));

        Assert.That(protos.Index(Evento).Components.TryGetComponent(nome, out var raw), Is.True);

        var evento = (StationEventComponent) raw!;

        Assert.Multiple(() =>
        {
            Assert.That(evento.StartAnnouncement, Is.Not.Null, "a tempestade tem que avisar antes");
            Assert.That(_loc.HasString(evento.StartAnnouncement!), Is.True,
                $"a chave {evento.StartAnnouncement} não existe no locale");
            Assert.That(evento.StartAudio, Is.Not.Null,
                "sem som o aviso passa despercebido no meio do chat");
        });

        await Task.CompletedTask;
    }

    /// <summary>
    /// Toda mensagem de estágio precisa existir, incluindo a do encerramento.
    /// Estágio com chave inventada não dá erro, sai a chave crua na tela.
    /// </summary>
    [Test]
    public async Task TodasAsMensagensDosEstagiosExistem()
    {
        var protos = Server.ProtoMan;
        var nome = Server.EntMan.ComponentFactory.GetComponentName(typeof(WeatherSchedulerComponent));

        Assert.That(protos.Index(Agendador).Components.TryGetComponent(nome, out var raw), Is.True);

        var agendador = (WeatherSchedulerComponent) raw!;
        var mensagens = agendador.Stages
            .Where(e => e.Message is not null)
            .Select(e => e.Message!.Value)
            .ToList();

        TestContext.Out.WriteLine($"estágios: {agendador.Stages.Count}, com mensagem: {mensagens.Count}");

        Assert.Multiple(() =>
        {
            foreach (var chave in mensagens)
            {
                Assert.That(_loc.HasString(chave), Is.True, $"a chave {chave} não existe no locale");
            }

            Assert.That(mensagens.Select(m => m.ToString()), Does.Contain("radiation-storm-over"),
                "falta o recado de que a ameaça passou, que é o que libera sair do abrigo");
        });

        await Task.CompletedTask;
    }
}
