using UnityEngine;

// 피아 구분 — Layer 충돌 매트릭스 대신 데이터 필드로 둠 (TagManager 캐시 깨짐 사고 회피).
public enum Faction { Player, Enemy }

// 피해 수신 계약 — 발사체(push)가 진영 확인 후 TakeDamage 호출. 구체 타입(Player/EnemyHealth) 결합 제거.
public interface IDamageable
{
    Faction Faction { get; }
    void TakeDamage(int amount);
}

// 접촉 피해 전용 — 적 몸통 등 "닿으면 아픈" 오브젝트에 부착. 수신부(예: PlayerHealth)가 OnTrigger에서 pull로 읽음.
// 발사체는 이 컴포넌트를 쓰지 않음 (push 모델 — Projectile이 직접 TakeDamage 호출).
public class DamageSource : MonoBehaviour
{
    public int damage = 1;
    public Faction faction = Faction.Enemy;
}
