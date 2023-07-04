using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public abstract class MonsterBaseState
    {
        protected MonsterState monsterState;

        /*각 상태는 자신이 속한 몬스터 인스턴스에 직접 액세스할 수 있으므로 
         * 몬스터의 동작, 속성을 수정하거나 해당 상태의 컨텍스트 내에서 필요한 작업을 수행
         * 각 상태는 Monster 클래스 구현의 세부 사항을 알 필요 없이 자체 논리를 캡슐화할 수 있기 때문에 
         * 우려 사항을 캡슐화하고 분리
         */
        protected MonsterBaseState(MonsterState monsterState)  //상태가 속한 특정 Monster 인스턴스에 대한 참조를 설정
        {
            this.monsterState = monsterState; 
        }

        public abstract void OnEnterState();
        public abstract void OnUpdateState();
        public abstract void OnExitState();
    }

}