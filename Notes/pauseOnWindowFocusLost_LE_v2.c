// NOTE: LOTD (original) has the same WM_ACTIVATEAPP handler, but doesn't pause the game... the "byte_140D326FA && byte_140D326F9" check is new


// Unpauses game
//----- (000000014083C420) ----------------------------------------------------
__int64 sub_14083C420()
{
  __int64 result; // rax@1

  result = (unsigned int)dword_143328028;
  if ( dword_143328028 & 2 )
  {
    result = (unsigned int)result & 0xFFFFFFFD;
    dword_143328028 = result;
  }
  return result;
}

// Pauses game
//----- (000000014083C9F0) ----------------------------------------------------
__int64 sub_14083C9F0()
{
  __int64 result; // rax@1

  result = (unsigned int)dword_143328028;
  if ( !(dword_143328028 & 2) )// <--- we will change this branch to skip pausing in the first place
  {
    result = (unsigned int)result | 2;// game pauses when this is 2
    dword_143328028 = result;
  }
  return result;
}



__int64 __usercall sub_1408CB610@<rax>(__int64 a1@<r13>, __int128 *a2@<xmm0>)
{
  char v2; // al@1
  void *v3; // rcx@2
  __int64 v4; // rax@5
  __int64 result; // rax@9

  v2 = byte_14332D430;
  byte_14332D430 = 0;
  if ( v2 )
  {
    v3 = sub_14087A160();
    if ( byte_140D326FA )
      sub_14087A270((__int64)v3, a1, a2);
    else
      sub_14087A3E0(1u, (__int64)v3, a1, a2);
    v4 = sub_14080AF80();
    if ( v4 )
      sub_14080B1D0(v4, *(float *)&a2);
  }
  if ( byte_140D326FA && byte_140D326F9 )// byte_140D326F9 denotes window focus
    result = sub_14083C420(); // <---- unpauses game
  else
    result = sub_14083C9F0(); // <---- pauses game
  return result;
}





// WndProc 0x1C (WM_ACTIVATEAPP) sets byte_140D326F9 (window focus)
//----- (00000001408CBC70) ----------------------------------------------------
int __fastcall sub_1408CBC70(__int64 a1, HWND a2, unsigned int a3, WPARAM a4, LPARAM lParam)
{
  WPARAM v5; // rdi@1
  UINT v6; // er14@1
  HWND v7; // r15@1
  __int64 v8; // rbx@1
  __int64 v10; // rax@19
  int v11; // ebx@22
  bool v12; // r8@26
  float v13; // xmm0_4@29
  int v14; // edx@29
  int v15; // ecx@31
  int v16; // edx@33
  int v17; // ecx@35
  __m128 v18; // xmm1@37
  __m128 v19; // xmm1@37
  char v20; // al@39
  __int64 v21; // [sp+0h] [bp-70h]@11
  __int128 v22; // [sp+20h] [bp-50h]@1
  __m128 v23; // [sp+30h] [bp-40h]@1
  __int128 v24; // [sp+40h] [bp-30h]@46
  __m128 v25; // [sp+50h] [bp-20h]@46
  struct tagPOINT Point; // [sp+60h] [bp-10h]@22
  __int64 v27; // [sp+68h] [bp-8h]@11

  v23.m128_i16[6] = 0;
  v23.m128_i32[2] = 0;
  v5 = a4;
  *(_QWORD *)&v22 = 0i64;
  v6 = a3;
  BYTE8(v22) = 0;
  v7 = a2;
  *(_QWORD *)((char *)&v22 + 12) = 0i64;
  v8 = a1;
  v23.m128_i8[4] = 0;
  if ( a3 > 0x200 )
  {
    switch ( a3 )
    {
      default:
LABEL_10:
        DefWindowProcW(v7, v6, v5, lParam);
        return sub_14090D410((unsigned __int64)&v21 ^ v27);
      case 0x20Au:
        if ( byte_140D326F9 )
        {
          Point.x = (signed __int16)lParam;
          Point.y = SWORD1(lParam);
          ScreenToClient(a2, &Point);
          v13 = *(float *)&qword_142924050;
          v22 = *(_OWORD *)&qword_142924050;
          v23 = *(__m128 *)((char *)&qword_14292405C + 4);
          sub_1408C2B00((__int64)&qword_140D2BE70, Point.x);
          LODWORD(v22) = (signed int)ffloor(v13);
          sub_1408C2B40((__int64)&qword_140D2BE70, Point.y);
          v12 = 1;
          v14 = v5 & 1 | 2;
          DWORD1(v22) = (signed int)ffloor(v13);
          if ( !(v5 & 0x10) )
            v14 = v5 & 1;
          v15 = v14 | 4;
          if ( !(v5 & 2) )
            v15 = v14;
          v16 = v15 | 8;
          if ( !(v5 & 0x20) )
            v16 = v15;
          v17 = v16 | 0x10;
          if ( !(v5 & 0x40) )
            v17 = v16;
          v23.m128_i32[0] = v17;
          v18 = _mm_shuffle_ps(v23, v23, -46);
          v18.m128_f32[0] = v23.m128_f32[2] + (float)(signed int)ffloor((float)SWORD1(v5) / 120.0);
          v19 = _mm_shuffle_ps(v18, v18, -55);
          goto LABEL_46;
        }
        return sub_14090D410((unsigned __int64)&v21 ^ v27);
      case 0x201u:
        if ( byte_140D326F9 )
        {
          sub_1408CAAF0((__int64)&v22, lParam, a4);
          v20 = BYTE8(v22);
          DWORD3(v22) |= 1u;
          v12 = 1;
          v23.m128_i8[12] = 1;
          if ( (unsigned __int8)byte_1429240B0 ^ ((unsigned __int8)v5 >> 3) & 1 )
            v20 = 1;
          BYTE8(v22) = v20;
          goto LABEL_45;
        }
        return sub_14090D410((unsigned __int64)&v21 ^ v27);
      case 0x202u:
        sub_1408CAAF0((__int64)&v22, lParam, a4);
        DWORD3(v22) &= 0xFFFFFFFE;
        break;
      case 0x204u:
        if ( !byte_140D326F9 )
          return sub_14090D410((unsigned __int64)&v21 ^ v27);
        sub_1408CAAF0((__int64)&v22, lParam, a4);
        DWORD3(v22) |= 4u;
        goto LABEL_44;
      case 0x205u:
        sub_1408CAAF0((__int64)&v22, lParam, a4);
        DWORD3(v22) &= 0xFFFFFFFB;
        break;
      case 0x207u:
        if ( !byte_140D326F9 )
          return sub_14090D410((unsigned __int64)&v21 ^ v27);
        sub_1408CAAF0((__int64)&v22, lParam, a4);
        DWORD3(v22) |= 2u;
        break;
      case 0x208u:
        sub_1408CAAF0((__int64)&v22, lParam, a4);
        DWORD3(v22) &= 0xFFFFFFFD;
        break;
    }
    v23.m128_i8[12] = 1;
LABEL_44:
    v12 = 1;
  }
  else
  {
    if ( a3 != 512 )
    {
      switch ( a3 )
      {
        case 0x1Cu:
          byte_140D326F9 = a4 == 1;// <---- denotes window focus
          if ( qword_14332BBB8 )
          {
            if ( a4 == 1 && !sub_1408CB310() && *(_DWORD *)(v8 + 56) == 1 )
            {
              *(_BYTE *)(v8 + 60) = 1;
              if ( !byte_140D326F8 )
                sub_1408CB820(v8);
            }
          }
          goto LABEL_10;
        default:
          goto LABEL_10;
        case 0x7Eu:
          LODWORD(qword_140D2BEA0) = (unsigned __int16)lParam;
          HIDWORD(qword_140D2BEA0) = WORD1(lParam);
          goto LABEL_10;
        case 5u:
          if ( byte_140D326FA != ((_WORD)a4 != 1) )
          {
            byte_140D326FA = (_WORD)a4 != 1;
            byte_14332D430 = 1;
          }
          LODWORD(qword_140D2BE90) = (signed __int16)lParam;
          HIDWORD(qword_140D2BE90) = SWORD1(lParam);
          goto LABEL_10;
        case 0x20u:
          if ( (_WORD)lParam != 1 || a4 != *(_QWORD *)(a1 + 64) )
            goto LABEL_10;
          sub_1408CA990(qword_14332D428);
          break;
        case 0x10u:
          v10 = sub_1408121A0();
          sub_1408189C0(v10);
          break;
        case 2u:
          PostQuitMessage(0);
          *(_QWORD *)(v8 + 64) = 0i64;
          break;
      }
      return sub_14090D410((unsigned __int64)&v21 ^ v27);
    }
    if ( !byte_140D326F9 )
      return sub_14090D410((unsigned __int64)&v21 ^ v27);
    v11 = qword_142924050;
    Point = (struct tagPOINT)qword_142924050;
    sub_1408CAAF0((__int64)&v22, lParam, a4);
    if ( (unsigned __int8)byte_1429240B0 ^ ((unsigned __int8)v5 >> 3) & 1
      && ((_DWORD)v22 != v11 || DWORD1(v22) != Point.y) )
    {
      BYTE8(v22) = 1;
    }
    v12 = (v11 - (signed int)v22) * (v11 - (signed int)v22) + (Point.y - DWORD1(v22)) * (Point.y - DWORD1(v22)) > 2;
  }
LABEL_45:
  v19 = v23;
LABEL_46:
  v25 = v19;
  v24 = v22;
  sub_140800DB0((__int64)&qword_142924010, (__int64)&v24, v12);
  return sub_14090D410((unsigned __int64)&v21 ^ v27);
}