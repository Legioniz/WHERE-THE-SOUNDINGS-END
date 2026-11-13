# Dialogue and Resident Interface

> Project: WHERE THE SOUNDINGS END  
> Status: Yarn Spinner dialogue presenter and four relationship-reactive residents implemented; Unity/Yarn runtime verification pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## Entry point

The player remains aboard the vessel. At a stationary shore approach, open the dockside hub with E, choose Harbour Office, Fish Market, Boat Repair Shed or Net Mender Shed, then choose the assigned resident. The hub closes before it hands off to GreyhavenRuntimeUI.BeginDialogue(node, npc).

Generated Greyhaven NPC trigger components are externally managed and do not compete for the shared Interact input. HarbourmasterInteraction remains available for hand-authored scenes.

## Presentation

GreyhavenRuntimeUI is the Yarn DialoguePresenterBase. The dialogue panel is positioned near the bottom of the screen and contains:
- Speaker label.
- Dialogue line.
- Choice buttons.
- Continue button (Space / Enter).
- Active resident relationship tier and 0–100 meter.

Click a choice or use keyboard number keys 1–5. Dialogue pauses scaled gameplay time. The active NpcAgent supplies relationship score, tier, CanTalk, CanTrade and buy-price policy; Yarn variables are synchronized from that resident before the conversation starts.

## Content contract

- Harbourmaster — Greyhaven_Harbourmaster.
- Mara Venn — Greyhaven_Fishmonger.
- Tomas Vale — Greyhaven_Mechanic.
- Inez Sarmiento — Greyhaven_NetMender.

The four nodes include seven trust-tier greetings, everyday topics, profession-specific facts, personal sayings and optional personal topics. Stable relationship event IDs are applied by C# once per resident. Critical story clues must not depend on a random schedule coincidence or require maximum trust.

## Interaction and persistence

The presenter closes dialogue and releases its pause when Yarn ends. NPC relationship scores and bounded event-memory IDs persist through NpcAgent; Yarn float/string/bool variables persist through YarnDialogueSaveParticipant. Mid-line presenter cursor state is not saved.

## Verification

Compile the Yarn project and test each node at hostile, neutral, familiar, trusted and ally scores in Unity 6000.3.28f1. Verify correct speaker labels, conditional choices, once-only relationship events, refusal below the conversation threshold, pause/resume, autosave checkpoint commands and no duplicate interaction start. Static C# parsing does not compile Yarn.
