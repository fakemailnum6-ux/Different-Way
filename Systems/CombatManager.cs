using Godot;
using System;
using System.Collections.Generic;
using DifferentWay.Core;
using DifferentWay.Systems.DataModels;

namespace DifferentWay.Systems
{
    public enum CombatState
    {
        Init,
        PlayerTurn,
        EnemyTurn,
        Resolve,
        End
    }

    public partial class CombatManager : Node
    {
        private CombatState _currentState;
        private DamageCalculator _damageCalculator;
        private StatusEffectManager _effectManager;
        private EventBus _eventBus;

        private CharacterStats _playerStats;
        private CharacterStats _enemyStats;
        private WeaponData _playerWeapon;
        private ArmorData _enemyArmor;

        private Dictionary<string, ActiveEffect> _playerEffects = new Dictionary<string, ActiveEffect>();
        private Dictionary<string, ActiveEffect> _enemyEffects = new Dictionary<string, ActiveEffect>();

        public delegate void CombatStateChangedEventHandler(CombatState state);
        public event CombatStateChangedEventHandler OnCombatStateChanged;

        public delegate void CombatUpdateEventHandler(string playerName, int php, int pmaxhp, string enemyName, int ehp, int emaxhp);
        public event CombatUpdateEventHandler OnCombatUpdate;

        public override void _Ready()
        {
            _damageCalculator = GetNodeOrNull<DamageCalculator>("/root/DamageCalculator");
            _effectManager = GetNodeOrNull<StatusEffectManager>("/root/StatusEffectManager");
            _eventBus = GetNodeOrNull<EventBus>("/root/EventBus");
        }

        public void StartCombat(CharacterStats player, CharacterStats enemy, WeaponData pWeapon, ArmorData eArmor)
        {
            _playerStats = player;
            _enemyStats = enemy;
            _playerWeapon = pWeapon;
            _enemyArmor = eArmor;
            _playerEffects.Clear();
            _enemyEffects.Clear();

            TransitionTo(CombatState.Init);
        }

        private void TransitionTo(CombatState newState)
        {
            _currentState = newState;
            OnCombatStateChanged?.Invoke(_currentState);

            UpdateUI();

            switch (_currentState)
            {
                case CombatState.Init:
                    OnInit();
                    break;
                case CombatState.PlayerTurn:
                    _eventBus?.EmitLogMessage("INFO", "Ваш ход.");
                    break;
                case CombatState.EnemyTurn:
                    _eventBus?.EmitLogMessage("INFO", "Ход противника.");
                    OnEnemyTurn();
                    break;
                case CombatState.Resolve:
                    break;
                case CombatState.End:
                    _eventBus?.EmitLogMessage("INFO", "Бой окончен.");
                    _eventBus?.EmitGameStateTransition("Exploration");
                    break;
            }
        }

        private void OnInit()
        {
            if (_damageCalculator == null) return;

            int pInit = _damageCalculator.CalculateInitiative(_playerStats);
            int eInit = _damageCalculator.CalculateInitiative(_enemyStats);

            if (pInit >= eInit)
            {
                TransitionTo(CombatState.PlayerTurn);
            }
            else
            {
                TransitionTo(CombatState.EnemyTurn);
            }
        }

        public void PlayerAttack()
        {
            if (_currentState != CombatState.PlayerTurn) return;
            ExecuteAttack(_playerStats, _enemyStats, _playerWeapon, _enemyArmor, true);
        }

        public void PlayerFlee()
        {
            if (_currentState != CombatState.PlayerTurn) return;

            bool success = _damageCalculator?.CheckFlee(_playerStats, _enemyStats) ?? false;
            if (success)
            {
                 _eventBus?.EmitLogMessage("INFO", "Вы успешно сбежали!");
                 TransitionTo(CombatState.End);
            }
            else
            {
                 _eventBus?.EmitLogMessage("WARNING", "Вам не удалось сбежать!");
                 ExecuteAttack(_enemyStats, _playerStats, null, null, false); // Free attack
            }
        }

        private void OnEnemyTurn()
        {
            ExecuteAttack(_enemyStats, _playerStats, null, null, false);
        }

        private void ExecuteAttack(CharacterStats attacker, CharacterStats defender, WeaponData weapon, ArmorData armor, bool isPlayer)
        {
            TransitionTo(CombatState.Resolve);

            if (_damageCalculator == null) return;

            var ctx = new CombatContext
            {
                Attacker = attacker,
                Defender = defender,
                Weapon = weapon,
                DefenderArmor = armor
            };

            bool hit = _damageCalculator.CheckHit(ctx);
            if (!hit)
            {
                _eventBus?.EmitLogMessage("INFO", isPlayer ? "Вы промахнулись!" : "Противник промахнулся!");
            }
            else
            {
                var dmgResult = _damageCalculator.CalculateDamage(ctx);
                if (dmgResult.isCritFail)
                {
                    _eventBus?.EmitLogMessage("WARNING", "Критический промах! Оружие повреждено.");
                }
                else
                {
                    defender.CurrentHP -= dmgResult.damage;
                    string critStr = dmgResult.isCrit ? " КРИТ!" : "";
                    _eventBus?.EmitLogMessage("INFO", $"Нанесено {dmgResult.damage} урона.{critStr}");
                }
            }

            // Tick status effects
            if (_effectManager != null)
            {
                _effectManager.TickEffects(_playerStats, _playerEffects, _eventBus, "Игрок");
                _effectManager.TickEffects(_enemyStats, _enemyEffects, _eventBus, "Противник");
            }

            UpdateUI();
            CheckWinConditions(isPlayer);
        }

        private void CheckWinConditions(bool wasPlayerTurn)
        {
            if (_playerStats.CurrentHP <= 0)
            {
                _eventBus?.EmitLogMessage("ERROR", "Вы погибли...");
                TransitionTo(CombatState.End);
            }
            else if (_enemyStats.CurrentHP <= 0)
            {
                _eventBus?.EmitLogMessage("INFO", "Враг повержен!");
                TransitionTo(CombatState.End);
            }
            else
            {
                TransitionTo(wasPlayerTurn ? CombatState.EnemyTurn : CombatState.PlayerTurn);
            }
        }

        private void UpdateUI()
        {
            if (_playerStats != null && _enemyStats != null)
            {
                OnCombatUpdate?.Invoke("Игрок", _playerStats.CurrentHP, _playerStats.MaxHP, "Противник", _enemyStats.CurrentHP, _enemyStats.MaxHP);
            }
        }
    }
}
