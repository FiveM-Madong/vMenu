# HB Admin Lite

`hb_adminlite`는 vMenu 관리자 기능을 가볍게 분리하기 위해 만든 FiveM 관리자 리소스입니다.

일반 유저에게 관리자 기능 Tick 부담을 최대한 주지 않는 구조를 목표로 합니다. 월드싱크 기능은 리소스 내부에 통합되어 있으며, vMenu DLL이나 vMenu 권한 구조에 의존하지 않습니다.

## 구성

- `hbAdminLiteClient/`: 클라이언트 C# 소스
- `hbAdminLiteServer/`: 서버 C# 소스
- `resource/hb_adminlite/`: FiveM 리소스 폴더
- `resource/hb_adminlite/ui/`: NUI 메뉴 파일
- `resource/hb_adminlite/config/`: PED/차량 카탈로그 및 권한 예시
- `hb_adminlite.sln`: HB Admin Lite 전용 솔루션

## 빌드

```powershell
dotnet build .\hb_adminlite.sln
```

빌드된 DLL은 아래 위치에 생성됩니다.

```text
resource/hb_adminlite/hbAdminLiteClient.net.dll
resource/hb_adminlite/hbAdminLiteServer.net.dll
```

DLL, `bin/`, `obj/` 등 빌드 산출물은 Git에 포함하지 않습니다.

## 설치

`resource/hb_adminlite` 폴더를 서버의 resources 폴더에 넣은 뒤 `server.cfg`에 추가합니다.

```cfg
ensure hb_adminlite
exec resources/[local]/hb_adminlite/permissions.cfg
```

리소스 위치가 다르면 `exec` 경로를 실제 위치에 맞게 수정하세요.

## 권한

권한은 `permissions.cfg`에서 관리합니다.

```cfg
add_principal identifier.license:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx group.admin
add_ace group.admin "hb_adminlite.Everything" allow
```

기능별 ACE도 사용할 수 있습니다.

```cfg
add_ace group.admin "hb_adminlite.NoClip" allow
add_ace group.admin "hb_adminlite.DevTools" allow
add_ace group.admin "hb_adminlite.PlayerOptions" allow
add_ace group.admin "hb_adminlite.PlayerAppearance" allow
add_ace group.admin "hb_adminlite.VehicleOptions" allow
add_ace group.admin "hb_adminlite.VehicleSpawner" allow
add_ace group.admin "hb_adminlite.WeaponOptions" allow
add_ace group.admin "hb_adminlite.WeaponLoadouts" allow
add_ace group.admin "hb_adminlite.OnlinePlayers" allow
```

`hb_adminlite.Everything`을 허용하면 모든 관리자 기능을 사용할 수 있습니다.

## 단축키

- `F10`: 관리자 메뉴 열기/닫기
- `F2`: 노클립 토글
- 방향키: 메뉴 이동
- `Enter`: 선택
- `Backspace` 또는 `\`: 뒤로가기
- `ESC`: 메뉴 닫기

노클립 기본 키는 convar로 바꿀 수 있습니다.

```cfg
setr hb_adminlite_noclip_key "F2"
```

## 월드싱크

월드싱크는 `hb_adminlite` 안에 통합되어 있습니다.

주요 convar:

```cfg
setr hb_worldsync_enabled true
setr hb_worldsync_enable_time_sync true
setr hb_worldsync_enable_weather_sync true
setr hb_worldsync_ingame_minute_duration 2000
setr hb_worldsync_current_weather "CLEAR"
```

시간/날씨 동기화를 끄고 싶으면 각각 false로 설정할 수 있습니다.

```cfg
setr hb_worldsync_enable_time_sync false
setr hb_worldsync_enable_weather_sync false
```

## 주의사항

- vMenu와 같이 사용할 목적이 아닙니다.
- 최종 구조는 vMenu 제거 후 `hb_adminlite`만 사용하는 것을 기준으로 합니다.
- `permissions.cfg`에 실제 운영 서버 식별자가 들어갈 수 있으니 공개 저장소에서 사용할 때는 필요에 따라 정리하세요.
- UI 파일은 UTF-8로 유지해야 합니다.
