﻿using Kermalis.PokemonBattleEngine.Battle;
using Kermalis.PokemonBattleEngine.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.AI
{
    /// <summary>Creates valid decisions for a team in a battle. Decisions may not be valid for custom settings and/or move changes.</summary>
    public static partial class PBEAI
    {
        /// <summary>Creates valid actions for a battle turn for a specific team.</summary>
        /// <param name="team">The team to create actions for.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="team"/> has no active battlers or <paramref name="team"/>'s <see cref="PBETeam.Battle"/>'s <see cref="PBEBattle.BattleState"/> is not <see cref="PBEBattleState.WaitingForActions"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a Pokémon has no moves, the AI tries to use a move with invalid targets, or <paramref name="team"/>'s <see cref="PBETeam.Battle"/>'s <see cref="PBEBattle.BattleFormat"/> is invalid.</exception>
        public static PBETurnAction[] CreateActions(PBETeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }
            if (team.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(team));
            }
            if (team.Battle.BattleState != PBEBattleState.WaitingForActions)
            {
                throw new InvalidOperationException($"{nameof(team.Battle.BattleState)} must be {PBEBattleState.WaitingForActions} to create actions.");
            }
            var actions = new PBETurnAction[team.ActionsRequired.Count];
            var standBy = new List<PBEPokemon>();
            for (int i = 0; i < actions.Length; i++)
            {
                PBEPokemon user = team.ActionsRequired[i];
                // If a Pokémon is forced to struggle, it is best that it just stays in until it faints
                if (user.IsForcedToStruggle())
                {
                    actions[i] = new PBETurnAction(user.Id, PBEMove.Struggle, GetPossibleTargets(user, user.GetMoveTargets(PBEMove.Struggle))[0]);
                }
                // If a Pokémon has a temp locked move (Dig, Dive, Shadow Force) it must be used
                else if (user.TempLockedMove != PBEMove.None)
                {
                    actions[i] = new PBETurnAction(user.Id, user.TempLockedMove, user.TempLockedTargets);
                }
                // The Pokémon is free to switch or fight (unless it cannot switch due to Magnet Pull etc)
                else
                {
                    // Gather all options of switching and moves
                    PBEPokemon[] availableForSwitch = team.Party.Except(standBy).Where(p => p.FieldPosition == PBEFieldPosition.None && p.HP > 0).ToArray();
                    PBEMove[] usableMoves = user.GetUsableMoves();
                    var possibleActions = new List<(PBETurnAction Action, double Score)>();
                    for (int m = 0; m < usableMoves.Length; m++) // Score moves
                    {
                        PBEMove move = usableMoves[m];
                        PBEType moveType = user.GetMoveType(move);
                        PBEMoveTarget moveTargets = user.GetMoveTargets(move);
                        PBETurnTarget[] possibleTargets = PBEMoveData.IsSpreadMove(moveTargets)
                            ? new PBETurnTarget[] { GetSpreadMoveTargets(user, moveTargets) }
                            : GetPossibleTargets(user, moveTargets);
                        foreach (PBETurnTarget possibleTarget in possibleTargets)
                        {
                            // TODO: RandomFoeSurrounding (probably just account for the specific effects that use this target type)
                            // TODO: Don't queue up to do the same thing (two trying to afflict the same target when there are multiple targets)
                            var targets = new List<PBEPokemon>();
                            if (possibleTarget.HasFlag(PBETurnTarget.AllyLeft))
                            {
                                targets.Add(team.TryGetPokemon(PBEFieldPosition.Left));
                            }
                            if (possibleTarget.HasFlag(PBETurnTarget.AllyCenter))
                            {
                                targets.Add(team.TryGetPokemon(PBEFieldPosition.Center));
                            }
                            if (possibleTarget.HasFlag(PBETurnTarget.AllyRight))
                            {
                                targets.Add(team.TryGetPokemon(PBEFieldPosition.Right));
                            }
                            if (possibleTarget.HasFlag(PBETurnTarget.FoeLeft))
                            {
                                targets.Add(team.OpposingTeam.TryGetPokemon(PBEFieldPosition.Left));
                            }
                            if (possibleTarget.HasFlag(PBETurnTarget.FoeCenter))
                            {
                                targets.Add(team.OpposingTeam.TryGetPokemon(PBEFieldPosition.Center));
                            }
                            if (possibleTarget.HasFlag(PBETurnTarget.FoeRight))
                            {
                                targets.Add(team.OpposingTeam.TryGetPokemon(PBEFieldPosition.Right));
                            }
                            double score;
                            if (targets.All(p => p == null))
                            {
                                score = -100;
                            }
                            else
                            {
                                score = 0d;
                                targets.RemoveAll(p => p == null);
                                PBEMoveData mData = PBEMoveData.Data[move];
                                switch (mData.Effect)
                                {
                                    case PBEMoveEffect.Attract:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Destiny knot
                                            if (target.IsAttractionPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.BrickBreak:
                                    case PBEMoveEffect.Dig:
                                    case PBEMoveEffect.Dive:
                                    case PBEMoveEffect.Fly:
                                    case PBEMoveEffect.Hit:
                                    case PBEMoveEffect.Hit__MaybeBurn:
                                    case PBEMoveEffect.Hit__MaybeBurn__10PercentFlinch:
                                    case PBEMoveEffect.Hit__MaybeConfuse:
                                    case PBEMoveEffect.Hit__MaybeFlinch:
                                    case PBEMoveEffect.Hit__MaybeFreeze:
                                    case PBEMoveEffect.Hit__MaybeFreeze__10PercentFlinch:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_ACC_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_ATK_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_DEF_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_SPATK_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_SPDEF_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_SPDEF_By2:
                                    case PBEMoveEffect.Hit__MaybeLowerTarget_SPE_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerUser_ATK_DEF_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerUser_DEF_SPDEF_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerUser_SPATK_By2:
                                    case PBEMoveEffect.Hit__MaybeLowerUser_SPE_By1:
                                    case PBEMoveEffect.Hit__MaybeLowerUser_SPE_DEF_SPDEF_By1:
                                    case PBEMoveEffect.Hit__MaybeParalyze:
                                    case PBEMoveEffect.Hit__MaybeParalyze__10PercentFlinch:
                                    case PBEMoveEffect.Hit__MaybePoison:
                                    case PBEMoveEffect.Hit__MaybeRaiseUser_ATK_By1:
                                    case PBEMoveEffect.Hit__MaybeRaiseUser_ATK_DEF_SPATK_SPDEF_SPE_By1:
                                    case PBEMoveEffect.Hit__MaybeRaiseUser_DEF_By1:
                                    case PBEMoveEffect.Hit__MaybeRaiseUser_SPATK_By1:
                                    case PBEMoveEffect.Hit__MaybeRaiseUser_SPE_By1:
                                    case PBEMoveEffect.Hit__MaybeToxic:
                                    case PBEMoveEffect.HPDrain:
                                    case PBEMoveEffect.Recoil:
                                    case PBEMoveEffect.Recoil__10PercentBurn:
                                    case PBEMoveEffect.Recoil__10PercentParalyze:
                                    case PBEMoveEffect.SuckerPunch:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Favor hitting ally with move if waterabsorb/voltabsorb etc
                                            // TODO: Liquid ooze
                                            // TODO: Check items
                                            // TODO: Stat changes and accuracy (even thunder/guillotine accuracy)
                                            // TODO: Check base power specifically against hp remaining (include spread move damage reduction)
                                            target.IsAffectedByMove(user, moveType, out double damageMultiplier, useKnownInfo: true);
                                            if (damageMultiplier <= 0) // (-infinity, 0.0] Ineffective
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                            else if (damageMultiplier <= 0.25) // (0.0, 0.25] NotVeryEffective
                                            {
                                                score += target.Team == team ? -5 : -30;
                                            }
                                            else if (damageMultiplier < 1) // (0.25, 1.0) NotVeryEffective
                                            {
                                                score += target.Team == team ? -10 : -10;
                                            }
                                            else if (damageMultiplier == 1) // [1.0, 1.0] Normal
                                            {
                                                score += target.Team == team ? -15 : +10;
                                            }
                                            else if (damageMultiplier < 4) // (1.0, 4.0) SuperEffective
                                            {
                                                score += target.Team == team ? -20 : +25;
                                            }
                                            else // [4.0, infinity) SuperEffective
                                            {
                                                score += target.Team == team ? -30 : +40;
                                            }
                                            if (user.HasType(moveType) && damageMultiplier > 0) // STAB
                                            {
                                                score += (user.Ability == PBEAbility.Adaptability ? 7 : 5) * (target.Team == team ? -1 : +1);
                                            }
                                        }

                                        break;
                                    }
                                    case PBEMoveEffect.Burn:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Heatproof, physical attacker
                                            if (target.IsBurnPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_ACC:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Accuracy, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_ATK:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_DEF:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Defense, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_EVA:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Evasion, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_SPATK:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.SpAttack, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_SPDEF:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.SpDefense, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_SPE:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Speed, mData.EffectParam, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Confuse:
                                    case PBEMoveEffect.Flatter:
                                    case PBEMoveEffect.Swagger:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Only swagger/flatter if the opponent most likely won't use it against you
                                            if (target.IsConfusionPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Growth:
                                    {
                                        int change = team.Battle.WillLeafGuardActivate() ? +2 : +1;
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, change, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpAttack, change, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.LeechSeed:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            if (target.IsLeechSeedPossible(useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.LightScreen:
                                    {
                                        score += team.TeamStatus.HasFlag(PBETeamStatus.LightScreen) || IsTeammateUsingEffect(actions, PBEMoveEffect.LightScreen) ? -100 : +40;
                                        break;
                                    }
                                    case PBEMoveEffect.LowerTarget_ATK_DEF_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, -1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Defense, -1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.LowerTarget_DEF_SPDEF_By1_Raise_ATK_SPATK_SPE_By2:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Defense, -1, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpDefense, -1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Attack, +2, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpAttack, +2, ref score);
                                            ScoreStatChange(user, target, PBEStat.Speed, +2, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.LuckyChant:
                                    {
                                        score += team.TeamStatus.HasFlag(PBETeamStatus.LuckyChant) || IsTeammateUsingEffect(actions, PBEMoveEffect.LuckyChant) ? -100 : +40;
                                        break;
                                    }
                                    case PBEMoveEffect.Moonlight:
                                    case PBEMoveEffect.Rest:
                                    case PBEMoveEffect.RestoreTargetHP:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            if (target.Team == team)
                                            {
                                                score += HPAware(target.HPPercentage, +45, -15);
                                            }
                                            else
                                            {
                                                score -= 100;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Nothing:
                                    case PBEMoveEffect.Teleport:
                                    {
                                        score -= 100;
                                        break;
                                    }
                                    case PBEMoveEffect.Paralyze:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Effectiveness with thunder wave/glare
                                            if (target.IsParalysisPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Poison:
                                    case PBEMoveEffect.Toxic:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Poison Heal
                                            if (target.IsPoisonPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_ATK_ACC_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Accuracy, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_ATK_DEF_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Defense, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_ATK_DEF_ACC_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Defense, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Accuracy, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_ATK_SPATK_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpAttack, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_ATK_SPE_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Speed, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_DEF_SPDEF_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Defense, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpDefense, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_SPATK_SPDEF_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.SpAttack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpDefense, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_SPATK_SPDEF_SPE_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.SpAttack, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.SpDefense, +1, ref score);
                                            ScoreStatChange(user, target, PBEStat.Speed, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.RaiseTarget_SPE_By2_ATK_By1:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            ScoreStatChange(user, target, PBEStat.Speed, +2, ref score);
                                            ScoreStatChange(user, target, PBEStat.Attack, +1, ref score);
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Reflect:
                                    {
                                        score += team.TeamStatus.HasFlag(PBETeamStatus.Reflect) || IsTeammateUsingEffect(actions, PBEMoveEffect.Reflect) ? -100 : +40;
                                        break;
                                    }
                                    case PBEMoveEffect.Safeguard:
                                    {
                                        score += team.TeamStatus.HasFlag(PBETeamStatus.Safeguard) || IsTeammateUsingEffect(actions, PBEMoveEffect.Safeguard) ? -100 : +40;
                                        break;
                                    }
                                    case PBEMoveEffect.Sleep:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            // TODO: Bad Dreams
                                            if (target.IsSleepPossible(user, useKnownInfo: true) == PBEResult.Success)
                                            {
                                                score += target.Team == team ? -20 : +40;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -60;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.Substitute:
                                    {
                                        foreach (PBEPokemon target in targets)
                                        {
                                            if (target.IsSubstitutePossible() == PBEResult.Success)
                                            {
                                                score += target.Team == team ? HPAware(target.HPPercentage, -30, +50) : -60;
                                            }
                                            else
                                            {
                                                score += target.Team == team ? 0 : -20;
                                            }
                                        }
                                        break;
                                    }
                                    case PBEMoveEffect.ChangeTarget_SPATK__IfAttractionPossible:
                                    case PBEMoveEffect.Conversion:
                                    case PBEMoveEffect.Curse:
                                    case PBEMoveEffect.Endeavor:
                                    case PBEMoveEffect.FinalGambit:
                                    case PBEMoveEffect.FocusEnergy:
                                    case PBEMoveEffect.GastroAcid:
                                    case PBEMoveEffect.Hail:
                                    case PBEMoveEffect.Haze:
                                    case PBEMoveEffect.HelpingHand:
                                    case PBEMoveEffect.HPDrain__RequireSleep:
                                    case PBEMoveEffect.MagnetRise:
                                    case PBEMoveEffect.Metronome:
                                    case PBEMoveEffect.OneHitKnockout:
                                    case PBEMoveEffect.PainSplit:
                                    case PBEMoveEffect.PowerTrick:
                                    case PBEMoveEffect.Protect:
                                    case PBEMoveEffect.PsychUp:
                                    case PBEMoveEffect.Psywave:
                                    case PBEMoveEffect.RainDance:
                                    case PBEMoveEffect.Sandstorm:
                                    case PBEMoveEffect.SeismicToss:
                                    case PBEMoveEffect.Selfdestruct:
                                    case PBEMoveEffect.SetDamage:
                                    case PBEMoveEffect.SimpleBeam:
                                    case PBEMoveEffect.Snore:
                                    case PBEMoveEffect.Soak:
                                    case PBEMoveEffect.Spikes:
                                    case PBEMoveEffect.StealthRock:
                                    case PBEMoveEffect.SunnyDay:
                                    case PBEMoveEffect.SuperFang:
                                    case PBEMoveEffect.Tailwind:
                                    case PBEMoveEffect.ToxicSpikes:
                                    case PBEMoveEffect.Transform:
                                    case PBEMoveEffect.TrickRoom:
                                    case PBEMoveEffect.Whirlwind:
                                    case PBEMoveEffect.WideGuard:
                                    {
                                        // TODO Moves
                                        break;
                                    }
                                    default: throw new ArgumentOutOfRangeException(nameof(PBEMoveData.Effect));
                                }
                            }
                            possibleActions.Add((new PBETurnAction(user.Id, move, possibleTarget), score));
                        }
                    }
                    if (user.CanSwitchOut())
                    {
                        for (int s = 0; s < availableForSwitch.Length; s++) // Score switches
                        {
                            PBEPokemon switchPkmn = availableForSwitch[s];
                            // TODO: Entry hazards
                            // TODO: Known moves of active battlers
                            // TODO: Type effectiveness
                            double score = -10d;
                            possibleActions.Add((new PBETurnAction(user.Id, switchPkmn.Id), score));
                        }
                    }

                    string ToDebugString((PBETurnAction Action, double Score) t)
                    {
                        string str = "{";
                        if (t.Action.Decision == PBETurnDecision.Fight)
                        {
                            str += string.Format("Fight {0} {1}", t.Action.FightMove, t.Action.FightTargets);
                        }
                        else
                        {
                            str += string.Format("Switch {0}", team.TryGetPokemon(t.Action.SwitchPokemonId).Nickname);
                        }
                        str += " [" + t.Score + "]}";
                        return str;
                    }
                    IOrderedEnumerable<(PBETurnAction Action, double Score)> byScore = possibleActions.OrderByDescending(t => t.Score);
                    Debug.WriteLine("{0}'s possible actions: {1}", user.Nickname, byScore.Select(t => ToDebugString(t)).Print());
                    double bestScore = byScore.First().Score;
                    actions[i] = byScore.Where(t => t.Score == bestScore).ToArray().RandomElement().Action; // Pick random action of the ones that tied for best score
                }
                // Action was chosen, finish up for this Pokémon
                if (actions[i].Decision == PBETurnDecision.SwitchOut)
                {
                    standBy.Add(team.TryGetPokemon(actions[i].SwitchPokemonId));
                }
            }
            return actions;
        }

        private static void ScoreStatChange(PBEPokemon user, PBEPokemon target, PBEStat stat, int change, ref double score)
        {
            // TODO: Do we need the stat change? Physical vs special vs status users, and base stats/transform stats/power trick stats
            sbyte original = target.GetStatChange(stat);
            sbyte maxStatChange = user.Team.Battle.Settings.MaxStatChange;
            change = Math.Max(-maxStatChange, Math.Min(maxStatChange, original + change)) - original;
            if (change != 0)
            {
                score += (user.Team == target.Team ? +1 : -1) * change * 10;
                score += HPAware(target.HPPercentage, -20, +10);
            }
        }
        private static bool IsTeammateUsingEffect(IEnumerable<PBETurnAction> actions, PBEMoveEffect effect)
        {
            return actions.Any(a => a != null && a.Decision == PBETurnDecision.Fight && PBEMoveData.Data[a.FightMove].Effect == effect);
        }
        private static double HPAware(double hpPercentage, double zeroPercentScore, double hundredPercentScore)
        {
            return ((-zeroPercentScore + hundredPercentScore) * hpPercentage) + zeroPercentScore;
        }

        /// <summary>Creates valid switches for a battle for a specific team.</summary>
        /// <param name="team">The team to create switches for.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="team"/> does not require switch-ins or <paramref name="team"/>'s <see cref="PBETeam.Battle"/>'s <see cref="PBEBattle.BattleState"/> is not <see cref="PBEBattleState.WaitingForActions"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="team"/>'s <see cref="PBETeam.Battle"/>'s <see cref="PBEBattle.BattleFormat"/> is invalid.</exception>
        public static PBESwitchIn[] CreateSwitches(PBETeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }
            if (team.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(team));
            }
            if (team.Battle.BattleState != PBEBattleState.WaitingForSwitchIns)
            {
                throw new InvalidOperationException($"{nameof(team.Battle.BattleState)} must be {PBEBattleState.WaitingForSwitchIns} to create switch-ins.");
            }
            if (team.SwitchInsRequired == 0)
            {
                throw new InvalidOperationException($"{nameof(team)} must require switch-ins.");
            }
            PBEPokemon[] available = team.Party.Where(p => p.FieldPosition == PBEFieldPosition.None && p.HP > 0).ToArray();
            available.Shuffle();
            var availablePositions = new List<PBEFieldPosition>();
            switch (team.Battle.BattleFormat)
            {
                case PBEBattleFormat.Single:
                {
                    availablePositions.Add(PBEFieldPosition.Center);
                    break;
                }
                case PBEBattleFormat.Double:
                {
                    if (team.TryGetPokemon(PBEFieldPosition.Left) == null)
                    {
                        availablePositions.Add(PBEFieldPosition.Left);
                    }
                    if (team.TryGetPokemon(PBEFieldPosition.Right) == null)
                    {
                        availablePositions.Add(PBEFieldPosition.Right);
                    }
                    break;
                }
                case PBEBattleFormat.Triple:
                case PBEBattleFormat.Rotation:
                {
                    if (team.TryGetPokemon(PBEFieldPosition.Left) == null)
                    {
                        availablePositions.Add(PBEFieldPosition.Left);
                    }
                    if (team.TryGetPokemon(PBEFieldPosition.Center) == null)
                    {
                        availablePositions.Add(PBEFieldPosition.Center);
                    }
                    if (team.TryGetPokemon(PBEFieldPosition.Right) == null)
                    {
                        availablePositions.Add(PBEFieldPosition.Right);
                    }
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(team.Battle.BattleFormat));
            }
            var switches = new PBESwitchIn[team.SwitchInsRequired];
            for (int i = 0; i < team.SwitchInsRequired; i++)
            {
                switches[i] = new PBESwitchIn(available[i].Id, availablePositions[i]);
            }
            return switches;
        }
    }
}
