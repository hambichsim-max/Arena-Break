# 내 프롬프트 로그

> **제출물입니다.** 
> 수업과 과제에서 실제로 사용한 프롬프트를 **전문 그대로** 기록하세요.
> **실패한 시도를 지우지 마세요.** 실패 기록이 회고 점수의 근거가 됩니다.

- 이름:심한빛
- 학번:2026103294

---

## 기록 형식

각 작업마다 아래 형식으로 남깁니다.

````markdown
## <장> — <무엇을 만들었나>

### 시도 1
```
(프롬프트 전문을 여기에 붙여넣기)
```
**결과**:
**문제점**:
**원인 분석**:

### 시도 2
```
(수정한 프롬프트 전문)
```
**무엇을 바꿨나**:
**결과**:
**배운 점**:
````

---

## 1장

### 1-1. CLAUDE.md 작성

<!-- 여기부터 기록 -->

### 1-2. 플레이어 이동
시도 1
1인칭 플레이어 컨트롤러를 만들어줘.

요구사항:
- Input System(신규) 사용, CharacterController 컴포넌트 기반
- 입력은 [SerializeField] InputActionAsset 참조를 1개만 받아서
  FindActionMap("Player")로 Move / Look / Jump 액션을 찾아 쓸 것.
  PlayerInput 컴포넌트나 Keyboard.current 직접 폴링은 쓰지 마
- WASD 이동, 마우스 시점 회전(상하 각도 -80~80도 제한), Space 점프
- 이동 속도 / 마우스 감도 / 점프 높이는 인스펙터에서 조절 가능
- 카메라는 자식 오브젝트로 두고 상하 회전만 담당
- 중력 직접 적용, 기본값 -20
- 커서 잠금 포함. 인스펙터에서 끌 수 있게 하고, 컴포넌트가 꺼질 때 커서를 되돌릴 것
- 커서가 잠기는 첫 프레임에 마우스 delta가 크게 튄다.
  프레임당 delta에 상한을 둘 것 (상한값은 인스펙터에서 조절)

파일: Assets/Scripts/Player/PlayerController.cs 하나만 생성.
씬은 건드리지 말고, 내가 직접 붙일 테니 필요한 세팅 절차만 알려줘.
결과:
Assets/Scripts/Player/PlayerController.cs를 생성했습니다. CharacterController 기반 WASD 이동과 점프, 중력, 마우스 시점 회전, 프레임당 마우스 delta 제한, 커서 잠금과 복원을 구현했습니다. Unity MCP 검증에서 오류는 없었습니다.
문제점:
스크립트의 카메라 참조 필드를 cameraTransform으로 만들었으나, 이후 씬 설정 요청에서는 Camera Pivot 필드를 지정해 달라고 했습니다. 필드명이 일치하지 않았고, Unity MCP가 PlayerController 타입을 예제 코드의 동명 타입과 혼동했습니다.
원인 분석:
첫 요청에 카메라 참조 필드의 이름이 정해져 있지 않아 cameraTransform을 사용했습니다. 다음 작업에서 구체적인 필드 이름이 필요해졌고, MCP의 타입 이름 검색에서도 클래스 이름 충돌이 드러났습니다.
시도 2
방금 만든 PlayerController를 씬에 붙여줘. MCP 도구로 직접 해줘.

- Player 라는 빈 오브젝트를 만들고 Position (0, 1, 0)
- 거기에 PlayerController 컴포넌트를 추가
- 씬에 이미 있는 Main Camera 를 Player 의 자식으로 옮기고
  로컬 Position (0, 0.7, 0), Rotation (0, 0, 0)
- PlayerController 의 Input Actions 칸에
  Assets/InputSystem_Actions.inputactions 를 연결
- Camera Pivot 칸에 그 Main Camera 를 연결

끝나면 컴포넌트를 다시 읽어서 두 칸에 실제로 값이 들어갔는지 보여줘.
무엇을 바꿨나:
카메라 참조 필드를 cameraPivot으로 변경하고, PlayerController를 ArenaBreak.Controllers 네임스페이스에 넣어 MCP의 타입 이름 충돌을 피했습니다. Unity MCP로 Player 오브젝트와 CharacterController를 생성하고, 기존 카메라를 자식으로 옮긴 뒤 Input Actions와 카메라 참조를 할당했습니다.
결과:
컴포넌트를 다시 읽어 inputActions에 Assets/InputSystem_Actions.inputactions, cameraPivot에 Main Camera가 연결된 것을 확인했습니다. 카메라는 로컬 위치 (0, 0.7, 0), 회전 (0, 0, 0)입니다.
배운 점:
씬에 컴포넌트를 연결하기 전 직렬화 필드 이름과 MCP에서 사용할 타입 이름을 맞춰야 합니다. 설정 후 컴포넌트를 다시 읽으면 실제 참조가 저장됐는지 확인할 수 있습니다.
시도 3
캡슐로 모델링 넣어줘
결과:
PlayerModel 캡슐을 Player의 자식으로 만들었습니다. 캡슐의 Collider는 비활성화했고, 루트의 CharacterController가 충돌을 담당하도록 했습니다.
문제점:
없음.
원인 분석:
캡슐 기본 Primitive에는 CapsuleCollider가 함께 추가되므로, Player 루트의 CharacterController와 충돌 처리가 겹치지 않도록 캡슐의 Collider를 껐습니다.


### 1-3. 사격 시스템

### 1-4. 과제 — 대시 또는 조준

---

## 2장

### 2-1. 체력/데미지 설계 논의

<!-- 프롬프트 전문 + 고른 안 + 고른 이유(본인의 이유) -->

### 2-2. 체력과 데미지 구현

<!-- WeaponSystem 수정분 git diff 에서 요청하지 않은 변경을 찾았다면 함께 기록 -->

### 2-3. 적 AI

### 2-4. 웨이브 시스템

### 2-5. 오브젝트 풀링 리팩터링

<!-- "밖에서 보면 아무것도 안 바뀌었는가" 에 대한 본인 판정 -->

### 2-6. 과제 — 원거리 적 또는 돌진 적

---

## 3장

### 3-1. 게임 상태 머신

### 3-2. HUD

### 3-3. 게임필

<!-- AI가 1번만 만들었는가, 3개를 다 만들었는가 -->

### 3-4. AI 코드 리뷰

<!-- 아래 표를 채울 것. 틀린 지적이 최소 1개는 있어야 한다 -->

| # | AI의 지적 | 내 판정 | 근거 |
| - | --------- | ------- | ---- |
| 1 |           |         |      |
| 2 |           |         |      |

### 3-5. 빌드

---

## 자체 점검

제출 전에 확인하세요.

```
□ 프롬프트를 요약하지 않고 전문 그대로 붙여넣었는가
□ 실패한 시도가 최소 3개 이상 기록되어 있는가
□ 각 실패마다 "왜 실패했는지" 분석이 있는가
□ 프롬프트를 어떻게 고쳤는지 비교할 수 있는가
```

> 실습 동안 한 번도 실패하지 않는 것은 불가능합니다.
> 실패 기록도 중요합니다.
