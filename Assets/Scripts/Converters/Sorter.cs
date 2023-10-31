using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����������
/// </summary>
public class Sorter : Converter
{
    private int _weightSum;
    private ConvertInfo[] Weights { get => base.CalculateResult(); }

    private void OnEnable()
    {
        CalculateSum();
        onConvertChange += CalculateSum;
    }

    //������ �������������� �� ��������� � ���������, ������������� ����������� ���������� ������ �� �����, ��������� � ���
    protected override ConvertInfo[] CalculateResult()
    {
        float point = Random.Range(0f, _weightSum);
        int curWeight = 0;
        ConvertInfo result = new ConvertInfo(ResourceType.Waste, 0);

        foreach (var weight in Weights) {
            curWeight += weight.amount;

            if (point <= curWeight) {
                result = new ConvertInfo(weight.type, 1);
                break;
            }
        }

        return new ConvertInfo[] { result };
    }

    /// <summary>
    /// ������ ����� �����
    /// </summary>
    private void CalculateSum()
    {
        _weightSum = 0;

        foreach (var weight in Weights)
            _weightSum += weight.amount;
    }
}
