// 被弾時に鈍化処理を持つ移動スクリプトが実装するインターフェース
// EnemyHP側はこのインターフェース経由で呼び出すので、
// 今後敵の移動スクリプトが増えてもEnemyHPを書き換える必要がない
public interface IHitSlowable
{
    void ApplyHitSlow();
}