using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance { get { return instance; } } //Get�� ������ �ۺ� ������Ƽ

    protected virtual void Awake()
    {
        if (instance == null)
        {
            //�ڽ��� ��쿡 T�� Ÿ���� ĳ�����Ͽ����� monobehaviour�� ����������� ���Բ� ���������� �����ؾ��Ѵ�.
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(this);
        }
    }
}
