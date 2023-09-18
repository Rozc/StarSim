

using Script.Objects;
using Script.Tools;
using UnityEngine;

namespace Script
{
    public class TargetSelector : SingletonBase<TargetSelector>
    {

        private int _cachedPositionEnemy = 6;
        private int _cachedPositionFriendly = 1;
        private bool _currentSelecingFriendly = false;
        public int CurrentPosition = 6;
        public bool Available = false;

        public void Move(int direction)
        {
            while (true)
            {
                // 移动时检测要移动的方向是否还有目标, 查看 GM 的列表
                if (GameManager.Instance.PosDict.TryGetValue(CurrentPosition + direction, out BaseObject obj))
                {
                    if (!obj.isAlive)
                    {
                        // 如果目标已经倒下, 则继续检测下一个目标
                        direction = direction + (int)Mathf.Sign(direction);
                        continue;
                    }

                    CurrentPosition += direction;
                }
                if (_currentSelecingFriendly) _cachedPositionFriendly = CurrentPosition;
                else _cachedPositionEnemy = CurrentPosition;
                break;
            }
        }

        // TODO 以下两个方法需要检查 cache 是否有效，以避免目标退场导致的位置空缺
        public void SelectingFriendlyTarget()
        {
            if (_currentSelecingFriendly) return;
            _currentSelecingFriendly = true;
            CurrentPosition = _cachedPositionFriendly;
        }

        public void SelectingEnemyTarget()
        {
            if (!_currentSelecingFriendly) return;
            _currentSelecingFriendly = false;
            CurrentPosition = _cachedPositionEnemy;
        }

        public void MoveTo(BaseObject obj)
        {
            
        }
        
    }
}
