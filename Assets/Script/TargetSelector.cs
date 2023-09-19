

using System.Collections.Generic;
using System.Linq;
using Script.Enums;
using Script.Objects;
using Script.Tools;
using UnityEngine;

namespace Script
{
    public class TargetSelector : SingletonBase<TargetSelector>
    {
        public int CurrentPosition = 6;
        public bool Available = true;
        
        private Dictionary<int, CursorController> _cursorDict = new();
        private int _cachedPositionEnemy = 6;
        private int _cachedPositionFriendly = 1;
        private bool _isSelectingFriendly = false;
        private TargetForm _currentTargetForm;
        private bool _locked;
        private GameManager GM = GameManager.Instance;

        public void Move(bool left)
        {
            if (!Available || _locked) return;
            if (left && GM.PosDict[CurrentPosition].TryGetLeft(out var obj))
            {
                MoveTo(obj, _isSelectingFriendly);
            }
            else if (!left && GM.PosDict[CurrentPosition].TryGetRight(out obj))
            {
                MoveTo(obj, _isSelectingFriendly);
            }
        }

        public void SetCursorForm(TargetForm targetForm, TargetSide targetSide)
        {
            if (!Available) return;
            if (targetForm == TargetForm.None) return;

            _locked = false;
            
            if ((targetSide == TargetSide.Friendly) == _isSelectingFriendly)
            {
                // 阵营不变
                MoveTo(GM.PosDict[CurrentPosition], _isSelectingFriendly, targetForm);
            }
            else
            {
                if (_isSelectingFriendly)
                {
                    _cachedPositionFriendly = CurrentPosition;
                    MoveTo(GM.PosDict[_cachedPositionEnemy], false, targetForm);
                }
                else
                {
                    _cachedPositionEnemy = CurrentPosition;
                    MoveTo(GM.PosDict[_cachedPositionFriendly], true, targetForm);
                }
            }
        }
        public void MoveTo(BaseObject obj, bool isFriendly, TargetForm targetForm = TargetForm.None)
        {
            if(!Available || _locked) return;
            if (!_cursorDict.TryGetValue(obj.Position, out var cursor)) return;
            if (targetForm != TargetForm.None) _currentTargetForm = targetForm;
            if (_currentTargetForm == TargetForm.None) return;

            if (_isSelectingFriendly != isFriendly)
            {
                if (_isSelectingFriendly)
                {
                    _cachedPositionFriendly = CurrentPosition;
                }
                else
                {
                    _cachedPositionEnemy = CurrentPosition;
                }
            }
            
            _isSelectingFriendly = isFriendly;
            DisableAllCursor();
            cursor.Show(true, _isSelectingFriendly);
            CurrentPosition = obj.Position;
            
            switch (_currentTargetForm)
            {
                case TargetForm.Single:
                    break;
                case TargetForm.Blast:
                    if (obj.TryGetLeft(out var o) && _cursorDict.TryGetValue(o.Position, out cursor))
                    {
                        cursor.Show(false, _isSelectingFriendly);
                    }

                    if (obj.TryGetRight(out o) && _cursorDict.TryGetValue(o.Position, out cursor))
                    {
                        cursor.Show(false, _isSelectingFriendly);
                    }

                    break;
                case TargetForm.Bounce:
                case TargetForm.Aoe:
                    if (_isSelectingFriendly)
                    {
                        var list = GM.FriendlyObjects;
                        foreach (var of in list.Where(o => o.Position != CurrentPosition))
                        {
                            _cursorDict[of.Position].Show(_currentTargetForm == TargetForm.Aoe, true);
                        }
                    }
                    else
                    {
                        var list = GM.EnemyObjects;
                        foreach (var oe in list.Where(o => o.Position != CurrentPosition))
                        {
                            _cursorDict[oe.Position].Show(_currentTargetForm == TargetForm.Aoe, false);
                        }
                    }

                    break;
                default:
                    Debug.LogError("Unknown Target Form!");
                    break;
            }
        }

        public void Lock()
        {
            _locked = true;
        }

        private void DisableAllCursor()
        {
            foreach (var cursor in _cursorDict.Values)
            {
                cursor.Hide();
            }
        }
        

        public void GetCursor(int pos, CursorController cursor)
        {
            _cursorDict.Add(pos, cursor);
        }

        public void Enable()
        {
            Available = true;
        }

        public void Disable()
        {
            Available = false;
            DisableAllCursor();
        }
        
    }
}
