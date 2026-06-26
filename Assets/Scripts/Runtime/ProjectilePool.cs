using System.Collections.Generic;
using UnityEngine;

// 단일 프리팹 오브젝트 풀 — 기관총 10발/초 연사의 Instantiate/Destroy GC 스파이크 방지.
// 무기별 비주얼 분화 전까지 전 무기가 같은 placeholder 발사체를 공유.
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] Projectile _prefab;
    [SerializeField] int        _initialSize = 30;

    readonly Queue<Projectile> _pool = new Queue<Projectile>();

    void Awake()
    {
        // 초기 워밍업 — 런타임 첫 발사 때 일괄 생성 비용을 로드 시점으로 당김
        for (int i = 0; i < _initialSize; i++)
            _pool.Enqueue(CreateInactive());
    }

    Projectile CreateInactive()
    {
        var p = Instantiate(_prefab, transform);
        p.gameObject.SetActive(false);
        return p;
    }

    // 비활성 발사체 반환 — 호출자가 Launch로 활성화. 풀 고갈 시 증설(상한 없음, 동시 발사 피크 대응)
    public Projectile Get()
    {
        return _pool.Count > 0 ? _pool.Dequeue() : CreateInactive();
    }

    public void Return(Projectile p)
    {
        p.gameObject.SetActive(false);
        _pool.Enqueue(p);
    }
}
