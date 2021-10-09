﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    public abstract void Stop();
    public abstract Transform SeeTarget();
    public abstract bool Hear();
    public abstract void Follow();
    public abstract void Seek();
    public abstract void Patrol();
    public abstract void ReturnToPatrol();
    public abstract void AddSoundSourceCache(Vector3 soundSourcePos, float soundVolume);
    public abstract void HearingDelayClear(Vector2 target);
    public abstract IPatrolComponent patrolComponent { get; set; }

}


public class ZombieEntity : Enemy
{
    //数据层(ScritableObject)
    public MoveSetting moveSetting;
    public NavMeshAgentSetting agentSetting;
    public SeeSetting seeSetting;
    public HearSetting hearSetting;
    public PatrolSetting patrolSetting;
    //public ZombieFSMSetting fsmSetting;

    public NavMeshAgent agent;
    

    //行为层(Component)
    //Move组件
    //IMoveComponent moveComponent;
    //AIMove组件
    //IAIMoveComponent aIMoveComponent;
    IMoveComponent AIMoveComponent;
    //See组件
    ISeeComponent seeComponent;
    //Hear组件
    IHearComponent hearComponent;
    //Patrol组件
    public override IPatrolComponent patrolComponent { get; set; }
    //Follow组件
    IFollowCompenent followComponent;
    //Seek组件
    ISeekComponent seekComponent;

    //逻辑层(FSM)
    //状态机
    IZombieFSM zombieFSM;
    public StateID defaultState = StateID.Pend;
    //状态指针
    public StateID currentState = StateID.Pend;


    /// <summary>
    /// 创建组件、依赖注入
    /// </summary>
    private void Start()
    {
        //零级行为层：触发器
        seeComponent = new SeeComponent(seeSetting, transform);
        hearComponent = new HearComponent(hearSetting);

        //一级行为层
        agent = gameObject.AddComponent<NavMeshAgent>();
        AIMoveComponent = new AIMoveComponent(agentSetting,agent);
        //moveComponent = new MoveComponent(moveSetting);

        //二级行为层：行为组合，依赖并调用一级行为层
        followComponent = new FollowComponent(AIMoveComponent);
        seekComponent = new SeekComponent(AIMoveComponent);
        patrolComponent = new PatrolComponent(patrolSetting,AIMoveComponent, transform);

        //逻辑层：行为组织,二级行为层间的转换机制
        zombieFSM = new ZombieFSM(this, defaultState);


        this.currentState = zombieFSM.currentState;

        TriggerCreater.instance.AddRidWithTemplate1(gameObject);

        
    }


    /// <summary>
    /// 行为层实现
    /// </summary>
    ///
    public override void Stop()
    {
        AIMoveComponent.Stop();
    }

    public override Transform SeeTarget()
    {
        return seeComponent.SeeTarget();
    }
    public override bool Hear()
    {
        return hearComponent.Hear();
    }

    public override void AddSoundSourceCache(Vector3 soundSourcePos, float soundVolume)
    {
        hearComponent.AddSoundSourceCache(soundSourcePos, soundVolume);
    }

    public override void HearingDelayClear(Vector2 target)
    {
        hearComponent.HearingDelayClear(target);

    }
    /*
    public override void SetNextTarget(PathNods nod)
    {
        patrolComponent.SetNextTarget(nod);
    }
    */
    public override void Follow()
    {
        if (seeComponent.SeeTarget() != null)
        {
            followComponent.Follow(seeComponent.SeeTarget());
        }
    }
    public override void Seek()
    {
        if (hearComponent.Hear())
        {
            seekComponent.Seek(hearComponent.HearTarget);
        }
    }
    public override void Patrol()
    {
        patrolComponent.Patrol();
    }
    public override void ReturnToPatrol()
    {
        patrolComponent.Return();
    }


    /// <summary>
    /// 逻辑层主循环
    /// </summary>
    public void Update()
    {
        //状态机更新
        zombieFSM.Update(gameObject);

        //音量记忆衰减
        hearComponent.HearWeaken();

        //Debug
        //print(gameObject.name+"当前的状态是：" + fsm.currentFSMState);
        //print("seekTarget="+theSeekTarget);

    }


}
