// SPDX-FileCopyrightText: 2026 Zequinza <felipe828218@gmail.com>
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Mood;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Whiskey.EntityEffects;

/// <summary>
/// Levanta um modificador de humor em quem tiver humor.
/// </summary>
/// <remarks>
/// Existe porque o porte do sistema de humor chegou sem nenhuma ponte para o
/// resto do jogo: nada em prototype nenhum levantava modificador. Até agora, as
/// únicas coisas que mexiam no humor eram o dano, por código, e os traços
/// periódicos. Isto abre o caminho para química, comida e qualquer outro efeito
/// de entidade fazerem o mesmo, sem cada um precisar do seu próprio sistema.
///
/// Não faz nada em quem não tem <c>MoodComponent</c>, e isso é de propósito.
/// Neste fork o humor é de quem escolheu um traço que dá humor, e não de todo
/// mundo, então remédio de humor não tem em que mexer numa pessoa comum.
/// </remarks>
/// <inheritdoc cref="EntityEffect"/>
public sealed partial class AdjustMood : EntityEffectBase<AdjustMood>
{
    /// <summary>
    /// Qual modificador levantar. O peso, a duração e a categoria são
    /// propriedades dele, no YAML, e não daqui: assim balancear é mexer em
    /// prototype e não em código.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MoodEffectPrototype> Effect;

    /// <summary>
    /// Multiplica o peso do modificador. Só vale para modificador SEM
    /// categoria: com categoria, o sistema de humor ignora, porque ali quem
    /// manda é a substituição por categoria.
    /// </summary>
    [DataField]
    public float Modifier = 1f;

    /// <summary>
    /// Soma ao peso depois da multiplicação. Mesma ressalva do
    /// <see cref="Modifier"/>.
    /// </summary>
    [DataField]
    public float Offset;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-adjust-mood", ("chance", Probability));
}
