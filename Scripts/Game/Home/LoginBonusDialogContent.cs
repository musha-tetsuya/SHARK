using UnityEngine;

public class LoginBonusDialogContent : MonoBehaviour
{
    /// <summary>
    ///  ダイアログ
    /// </summary>
    private SimpleDialog dialog;
    /// <summary>
    /// loginBonusIcon Prefab
    /// </summary>
    [SerializeField]
    private LoginBonusIcon loginBonusIcon = null;
    /// <summary>
    /// 生成位置
    /// </summary>
    [SerializeField]
    private RectTransform parentsArea = null;
    /// <summary>
    /// アイコンチェックイメージ
    /// </summary>
    private bool check = false;

    /// <summary>
    /// ダイアログセット
    /// </summary>
    public void Set(SimpleDialog dialog, uint checkCount)
    {
        this.dialog = dialog;
        var LoginBonusMaster = Masters.LoginBonusDB;

        // マスターのid数だけ生成
        for (int i = 0; i < LoginBonusMaster.GetList().Count; i++)
        {
            var icon = Instantiate(this.loginBonusIcon, this.parentsArea);
            var master = LoginBonusMaster.FindById((uint)i + 1);

            // チェックイメージbool判断
            if(checkCount > i)
            {
                this.check = true;
            }
            else
            {
                this.check =false;
            }

            // アイコン情報セット
            icon.Set(master, this.check);
        }
    }

    /// <summary>
    /// ダイアログセット
    /// </summary>
    public void SetSpecialLoginBonus(SimpleDialog dialog, uint checkCount)
    {
        this.dialog = dialog;
        var LoginBonusMaster = Masters.LoginBonusSpecialDB;

        // マスターのid数だけ生成
        for (int i = 0; i < LoginBonusMaster.GetList().Count; i++)
        {
            var icon = Instantiate(this.loginBonusIcon, this.parentsArea);
            var master = LoginBonusMaster.FindById((uint)i + 1);

            // チェックイメージbool判断
            if(checkCount > i)
            {
                this.check = true;
            }
            else
            {
                this.check =false;
            }

            // アイコン情報セット
            icon.SetSpecialLoginBonus(master, this.check);
        }
    }
    
    /// <summary>
    /// ダイアログ終了
    /// </summary>
    public void CloseButton()
    {
        this.dialog.Close();
    }
}
