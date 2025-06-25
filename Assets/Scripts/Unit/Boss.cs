using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : Enemy
{
    public float maxHealth = 300f;
    public float defaultHealth = 100f;
    // public HealthBar healthBar; // 필요하다면 Boss 프리팹에 HealthBar를 붙이고 할당

    void Start()
    {
        Debug.Log("[Boss] Start 호출됨");
        // healthBar = GetComponentInChildren<HealthBar>(); // 필요하다면 사용
    }

    public override void SetStat(int difficulty)
    {
        maxHealth = defaultHealth * difficulty * 3;
        health = maxHealth;
        Debug.Log($"[Boss] 스탯 설정 - 난이도: {difficulty}, 최대 체력: {maxHealth}");
        // if (healthBar != null)
        //     healthBar.UpdateHealthBar(health, maxHealth);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        Debug.Log($"[Boss] 데미지 받음! 체력: {health}/{maxHealth}");
        // if (healthBar != null)
        //     healthBar.UpdateHealthBar(health, maxHealth);
    }

    // Enemy의 Die() 메서드를 오버라이드
    protected override void Die()
    {
        Debug.Log("[Boss] 보스 사망! Main 씬으로 이동합니다.");
        
        // 게임 정지 해제 (이전 코드에서 설정된 경우)
        Time.timeScale = 1f;
        
        // Main 씬으로 이동
        SceneManager.LoadScene("main");
        
        Debug.Log("[Boss] Main 씬으로 이동 완료");
    }

    // 수동으로 게임 종료 테스트 (디버깅용)
    [ContextMenu("테스트 보스 사망")]
    public void TestBossDeath()
    {
        Debug.Log("[Boss] 테스트 보스 사망 호출됨");
        Die();
    }
}