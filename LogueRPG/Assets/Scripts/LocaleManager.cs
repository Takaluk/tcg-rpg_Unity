using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    bool isChanging;

    public void ChangeLocale(int index)
    {
        if (isChanging)
            return;

        StartCoroutine(ChaingeRoutine(index));
    }

    IEnumerator ChaingeRoutine(int index)
    {
        isChanging = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        isChanging = false;

        GameManager.instance.StartGame();

        gameObject.SetActive(false);
    }
}
