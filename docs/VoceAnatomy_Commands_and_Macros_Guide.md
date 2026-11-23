# VoceAnatomy Commands and Macros Guide

Nov 1, 2026 · @Steve DiPaola

## Overview

Every way of controlling VoceAnatomy now speaks one command language: what you type in the console, what you say by voice, and what you write in a macro file all become the same commands, run by the same engine. Learn a command once and it works everywhere.

What this gives you:

- **Any body part by name.** Over 3,000 parts, in English, Latin, French, Spanish or Portuguese, plus synonyms and everyday names.
- **Sequences that wait.** Each command finishes (a rotation, a camera move, a slice sweep) before the next one starts.
- **Macros.** A plain text file of commands, run by typing or saying its name. Experts can write and edit them without rebuilding the app.
- **Teaching tools.** On-screen captions and titles, part descriptions, and step-through lessons that wait for the presenter's click.

This guide covers the version on the `dev` branch as of the date above. Voice recognises the original command set plus **"macro <name>"**, which runs any macro hands-free (see Running a macro by voice). Other commands below are available by typing and in macros.

## The command console

Press the backtick key (`` ` ``, top left, under Esc) to open the console; type a command and press Enter. It covers the top of the screen and stays open, ready for the next command.

| Key | What it does |
|---|---|
| `` ` `` | Open or close the console |
| Enter | Run the line |
| Esc | Close the console |
| Up / Down | Step through earlier commands (kept between sessions) |
| Tab | Complete a command, macro or part name; with several matches, lists them in the log |

While you type, the app's hotkeys are off: V (push-to-talk) and Space (isolate) won't fire. The middle mouse button still starts voice.

**Reading the log.** Every command from every source appears here, colour-coded:

- **Blue** — a command as it runs (`> show heart`); commands inside a macro are indented.
- **Grey** — what happened, e.g. `hart -> Heart (17 parts)`, so you can see how a name was understood.
- **Purple** — voice: what was heard, how it was cleaned up, and the command it became.
- **Yellow / red** — warnings and errors, with line numbers for macros.

Everything is also written to Unity's Console with a `[Cmd]` prefix. Type `help` for every command, or `help <command>` (e.g. `help slice`) for one.

## Command syntax

A command is a verb, then what it acts on: `show heart`, `slice at 40`, `rotate slow`. The same rules apply in the console and in macro files.

| Rule | Example |
|---|---|
| One command per line; `;`, `then` and `and` also separate commands | `show heart; focus then rotate slow` |
| Commas or `and` list several parts for one command | `show heart, aorta and lungs` |
| Numbers as digits or words | `slice at 40` = `slice at forty` |
| Quotes keep text exactly as written (no splitting on `and` / `then` / `;`) | `caption "The heart, then the lungs."` |
| `#` starts a comment | `reset   # start clean` |
| Leading line numbers are ignored | `1 reset`, `2. show heart` |
| A trailing `&` starts a command and moves straight on | `rotate slow &` |
| `queue` waits for whatever is running before starting | `queue show lungs` |

**Waiting and interrupting.** Each command waits for the previous one to finish, including camera moves, rotations and slice sweeps. A new command typed or spoken while a macro (or a multi-command line) is running stops it and runs instead. A single command still animating, such as a lone `rotate`, is left to finish, so live commands can overlap. `stop` cancels everything at any time.

**Parallel steps.** `&` lets two things happen at once:

```
rotate slow &
slice at 40    # slices while the body turns
```

**Mistakes are reported, not guessed.** An unknown command suggests the nearest one (`rotat` → did you mean "rotate"?). A bad line in a macro is reported with its line number and skipped; the rest still runs.

## Naming body parts

Any of the model's ~3,000 named parts can be used in a command, and the lookup forgives everyday names, other languages and small misspellings. The log always shows what a name became (`hart -> Heart (17 parts)`), so you can check.

Names are tried in this order; the first that matches wins:

1. **Exact name or synonym** — `heart`, `skull` (synonym of Cranium), `the aorta`. English and the app's current language come before other languages, so `femur` is the bone, not the Latin name of the thigh.
2. **Alias or group** — from `aliases.json` and `groups.json` (below): `collarbone`, `spine`, `heart chambers`.
3. **Singular or plural** — `lung` finds Lungs, `kidneys` finds Kidney.
4. **Start of a name** — `cran` finds Cranium.
5. **All words present, any order** — for longer names.
6. **Close spelling** — `hart` → Heart, `clavical` → Clavicle.

Other languages work too: `coeur` and `crâne` (or `crane`) find the heart and the cranium.

**Left and right.** `kidney` means both kidneys; `left kidney` or `kidney right` means one. Names that contain a side word are kept whole, so `left ventricle` is the heart chamber.

**Groups.** Naming a group selects everything in it: `heart` is 17 parts, `brain` about 250, `aorta` 80.

**Ambiguous names are never guessed.** If a name fits several different parts equally well, nothing happens and the console lists them, e.g. `show ventricle` → *could be: Left ventricle, Right ventricle, Third ventricle, Fourth ventricle….* A name that matches nothing suggests the closest ones. Use **Tab** after `show`, `hide`, `isolate`, `focus`, `label` or `describe` to complete a part name.

### Adding your own names: aliases.json and groups.json

Two text files in `StreamingAssets/anatomy/` (they ship beside the built app) let experts add names without code. Both use the same shape: a name, and the parts it stands for.

```json
{
  "aliases": [
    { "name": "collarbone", "parts": ["clavicle"] },
    { "name": "pelvis",     "parts": ["hip bone", "sacrum", "coccyx"] }
  ]
}
```

- **aliases.json** — everyday or spoken names for parts: collarbone, shoulder blade, kneecap, spine, jaw, pelvis, windpipe, gullet, bladder, intestines, vena cava, biceps, triceps, abs, pecs, lats, glutes, traps, calf muscles, and more.
- **groups.json** — named teaching sets (same format, top-level key `"groups"`): heart chambers, rotator cuff, quadriceps (and quads), hamstrings, shoulder girdle, forearm bones, lower leg bones, great vessels, coronary arteries, mediastinum.

An alias can point at another alias or group (`quads` → `quadriceps`), and `left` / `right` work with aliases (`left collarbone`). Every entry is checked when the app starts: a part name that doesn't exist, or matches several parts, is reported in the console. Edit the files in any text editor; changes take effect the next time the app starts.

## Command reference

**One rule to remember: the last thing you named is what you're working on.** A command that names parts makes them the current selection; a command that names nothing acts on the selection. So `show heart` selects the heart, and a following `focus`, `rotate`, `label`, `describe` or `isolate` works on it. This is the app's normal selection: a part clicked with the mouse, named in a command, or set by a macro is the same thing, and the undo button still works.

Naming a whole body system (`show muscles`) changes what's visible but clears the selection, so `focus` then frames everything visible.

### Parts and selection

| Command | What it does | Example |
|---|---|---|
| `show <parts>` (also `select`, `add`) | Make parts or a system visible and select the parts | `show left kidney` |
| `also <parts>` | Show parts and add them to the selection | `show heart; also aorta` |
| `isolate [parts]` | Show only these parts (or the selection), hiding everything else; same result every time it runs. Systems can be mixed in to keep them too | `isolate heart, blood vessels, nerves` |
| `hide [parts]` (also `remove`) | Hide parts or a system; alone, hides the selection | `hide ribs` |
| `deselect` (also `clear selection`) | Clear the selection | `deselect` |
| `label [on/off] [parts]` | Show or hide the model's text labels on the parts or selection; `label off` alone hides all labels | `label femur` |
| `show/hide <system>` | Systems: muscles, skeleton, nerves, organs, blood vessels, lymph, joints | `hide skeleton` |

### Camera

| Command | What it does | Example |
|---|---|---|
| `focus [parts]` | Frame the selection (or the named parts); nothing selected frames everything visible | `focus aorta` |
| `zoom in/out [percent]` | Change distance by a percentage; plain `zoom in` frames the selection, plain `zoom out` deselects and frames the whole body | `zoom in 20` |
| `rotate [slow / degrees]` (also `turn`, `spin`) | One full turn (2 s), a slow turn (4 s), or turn N degrees and stop | `rotate 90` |
| `pan up/down/left/right [percent]` | Move the view, default 15% | `pan left 20` |
| `view front/back/left/right/top/bottom` | Turn smoothly (about 1 s) to a standard view, keeping zoom; a macro waits for the turn; `front` alone also works | `view back` |

### Cross-section

| Command | What it does | Example |
|---|---|---|
| `slice` / `slice on` | Turn on the front-to-back slice at the front (0%) | `slice on` |
| `slice at <percent>` | Jump to a depth; 0% is the front, 100% the back | `slice at 40` |
| `slice <percent>` | Step that much deeper | `slice 10` |
| `slice back` | Step 10% toward the front | `slice back` |
| `slice slow` (also `slow slice`) | Sweep through and back (about 14 s) | `slice slow` |
| `slice off` (also `remove slice`) | Remove the slice | `slice off` |

### Teaching (see Teaching lessons)

| Command | What it does | Example |
|---|---|---|
| `caption [seconds] "text"` / `caption clear` | Narration in the lower third | `caption "Four chambers."` |
| `title [seconds] "text"` / `title clear` | Large heading at the top | `title "The Heart"` |
| `describe [part]` | Opening of the part's description, in the caption | `describe aorta` |
| `wait click` / `wait key` | Hold until the presenter advances | `wait click` |

### Flow and macros

| Command | What it does | Example |
|---|---|---|
| `wait <seconds>` | Pause | `wait 3` |
| `stop` | Cancel everything running, including animations | `stop` |
| `run <macro> [part]`, or just the macro's name | Run a macro; by voice, say "macro" + its name | `heart intro` |
| `macros` | List macros; templates show `<part>` | `macros` |
| `check [macro]` | Report problems in one or all macros, with line numbers | `check` |
| `record <name>` … `end` | Save what you type or say as a new macro | `record heart tour` |
| `queue <commands>` | Wait for what's running, then run | `queue rotate` |

### Session

| Command | What it does | Example |
|---|---|---|
| `reset` (also `start over`, `whole body`) | Restore the whole body and camera; clears captions | `reset` |
| `reset view` | Reset the camera only | `reset view` |
| `clear` | Remove the slice and face front, keeping zoom and visibility | `clear` |
| `gesture off/leap/video` | Switch hand-tracking input | `gesture leap` |
| `help [command]` | List commands, or explain one | `help rotate` |
| `say "text"` | Test a phrase through the voice interpreter without a microphone | `say "show me the heart"` |

## Macros

A macro is a plain text file of commands, one per line, saved in `StreamingAssets/macros/` with the extension `.macro`. The file name is the macro's name: `heart_tour.macro` runs as `heart tour` (spaces, underscores and dashes are treated alike). The folder ships beside the built app, so macros can be written and changed in any text editor without rebuilding, and an edit takes effect the next time the macro runs.

These macros come with the app:

| Macro | What it shows |
|---|---|
| `demo` | A short tour: show and focus the heart, rotate, slice, zoom out |
| `heart intro` | A step-through lesson with a title, captions, a description and clicks to advance |
| `heart lungs tour` | Hands-free, about 90 s: how the heart and lungs work together, following the blood round the loop |
| `blood tour` | Hands-free, about 2.5 min: follows the blood from the heart to the lungs, brain, gut and liver, kidneys and legs, with slices and 4-second captions (see the example below) |
| `chest pain tour` | Step-through lesson for the Week 19 chest-pain case: pleura, mediastinum, heart and great vessels, then each coronary artery and its territory, tied to angina |
| `explain <part>` | A template: isolates, focuses, describes and rotates whatever part you name |

### Running a macro by voice

Hold **V** (or the middle mouse button) and say **"macro"** followed by the macro's name. The console doesn't need to be open.

| Say | Runs |
|---|---|
| "macro heart intro" | `heart_intro.macro` |
| "macro blood tour" | `blood_tour.macro` |
| "macro explain aorta" | the `explain` template on the aorta |
| "reset and macro demo" | `reset`, then `demo.macro` |

- **"macro" comes first.** Everything after it is taken as the name, so a name's own words don't trigger other commands. "macro slice demo" won't also turn the slice on.
- **Close is good enough.** If Whisper mishears the name ("hard intro"), the closest macro file is used. Only macros that exist can match, so a near miss never runs anything else. If nothing is close, the console says *no macro sounds like "…"* and lists the macros.
- **Name files to be spoken.** Two or three plain words that sound different from each other (`blood_tour`, `heart_intro`) work best. Avoid names that differ by a single sound.
- **New macros.** Macros present when the app starts are added to Whisper's vocabulary, which helps it hear their names. A macro saved during the session (by `record` or in a text editor) runs by voice straight away, but only joins that vocabulary after a restart.
- **Interrupting.** Any new spoken command stops a running macro and runs instead. There's no spoken "stop" yet; type `stop`.
- **Testing without a microphone.** `say "macro blood tour"` shows in the console exactly what the phrase would run.

### Writing and running a macro

1. Create a file, e.g. `StreamingAssets/macros/kidney_tour.macro`:

   ```
   # kidney_tour.macro -- run with "kidney tour"
   reset
   isolate kidney; focus
   rotate slow
   show left kidney; focus
   describe
   wait 5
   zoom out
   ```

2. In the console, type `check kidney tour` to look for mistakes.
3. Run it with `kidney tour` or `run kidney tour`. `stop` ends it early; any new command interrupts it.

### Checking

Every line and every part name is checked when a macro runs, and all macros are checked a few seconds after the app starts, so problems show up in the console before a lesson rather than halfway through. `check` checks them all on demand; `check <macro>` checks one. Each problem names the macro and line:

```
macro kidney tour, line 3: unknown command "rotat slow" -- did you mean "rotate"?
macro kidney tour, line 5: describe ventricle: "ventricle" could be: Left
ventricle, Right ventricle, ...
```

A bad line is skipped when the macro runs; the other lines still run.

### Recording

The quickest way to draft a lesson is to perform it once:

1. `record kidney tour` — recording starts.
2. Type or say the commands, as you would in class.
3. `end` — saves them to `kidney_tour.macro` and tells you the name to run it with.

Then open the file and tidy it: add `wait` steps, captions and comments. Commands about the session itself (`help`, `macros`, `check`, `stop`) aren't recorded, and recording never overwrites an existing macro; it saves as `kidney_tour_2.macro` instead.

### Templates: making your own commands

A macro that contains `{part}` becomes a new command that takes a part name. Whatever follows the macro's name replaces `{part}`:

```
# explain.macro -- "explain aorta" or "explain left kidney"
isolate {part}; focus
describe
rotate slow
```

`explain aorta` then works just like a built-in command. `macros` lists templates as `explain <part>`, and they're checked with a sample part filled in.

### Macros inside macros

A macro can run another, so a lecture can be built from smaller lessons:

```
# cardiology_lecture.macro
heart intro
explain aorta
explain left ventricle
```

Nesting stops at 8 levels, which catches a macro that accidentally runs itself.

## Teaching lessons

A lesson is a macro that narrates itself on screen and waits for the teacher between steps. Four commands do this.

**Captions** appear in a box in the lower third of the screen, in large type, separate from the 3D labels.

- `caption "The heart has four chambers."` — stays until the next caption or `caption clear`.
- `caption 5 "The aorta carries blood to the body."` — shows for 5 seconds while the macro waits, then disappears.
- `*word*` makes text **bold** and `_word_` makes it *italic*: `caption "The *left ventricle* pumps hardest."`
- Always put caption text in quotes. Without them, words like "then" and "and" split the text into separate commands.

**Titles** are a larger heading at the top, for section breaks: `title "The Cardiovascular System"`, removed with `title clear`. Both captions and titles fade in and out, and `reset` clears them.

**Descriptions.** `describe` puts the opening two sentences of the selected part's description into the caption, under its name; `describe aorta` names the part directly (and selects it). These are the descriptions the app already has, so you don't need to retype them. The full text still appears in the app's description panel.

**Waiting for the presenter.**

- `wait click` holds the lesson until the presenter clicks the model, or presses →, Page Down or Enter (the keys presentation clickers send). A Leap or video-gesture pinch counts as a click.
- While it waits, clicking doesn't select parts, so the advancing click doesn't change the lesson. A yellow "Click or → to continue" hint shows in the bottom-right corner; when it's showing, the lesson is waiting for you.
- `wait key` advances on the keys only, so the presenter can still click parts while talking.
- The arrow keys don't advance a lesson while the console has keyboard focus (they move the cursor there). Close the console with Esc during a lesson, or click the model.

### Example: a step-through lesson

This is `heart_intro.macro`, which comes with the app. Run it with `heart intro`, close the console, then click or press → to move through it.

```
# heart_intro.macro -- a step-through lesson. Run with "heart intro".
reset
title "The Heart"
isolate heart; focus
caption "The heart has *four chambers*: two atria above, two ventricles below."
wait click

isolate heart chambers; focus
caption "The atria receive blood; the ventricles pump it out."
wait click

show left ventricle; focus
describe
rotate slow
wait click

caption clear; title clear
zoom out
```

Each block is one step: set the view, narrate, wait. Blank lines are only for readability.

### Example: a hands-free tour

`blood_tour.macro` runs on its own, with no clicks. One section of it shows the patterns that keep a tour moving:

```
title "The Heart"
isolate heart, blood vessels, nerves; focus
caption 4 "Nerves speed it up or slow it down. Vessels carry everything it pumps."
isolate heart
rotate slow &
caption 4 "A fist-sized pump that beats about *100,000 times a day*."
slice at 40
caption 4 "Cut open: four chambers. The *left ventricle* has the thickest wall."
slice off
show blood vessels, nerves
```

- **`caption 4 "…"`** shows the text for 4 seconds and the macro waits that long, so captions set the pace with no `wait` lines.
- **`title "…"`** with no seconds stays up until the next title, so each section keeps its heading without adding a pause.
- **`rotate slow &`** turns the model while the next caption is showing.
- **Context, then focus.** The section opens with the organ inside the full network of vessels and nerves, `isolate heart` strips the network away, and `show blood vessels, nerves` brings it back before the next section, so the camera moves happen with the network visible.
- **Slice depths** are a percentage of the whole body, front to back. The heart cuts well at about 40%; organs near the back, like the kidneys, need a higher number.
- **`# STUDY:` comments** hold extra detail for the presenter; they don't show on screen.

## Tips and troubleshooting

| Problem | What to do |
|---|---|
| A name isn't found | Look at the suggestions in the log, press Tab to complete it, or add an everyday name to `aliases.json`. |
| "could be: …" | The name fits several parts; use the full name from the list (`left ventricle`, not `ventricle`). |
| `label <part>` says there are no labels | Only about 114 parts have text labels in the model, mostly bones. |
| A macro stops part way | Read the yellow/red lines in the console; run `check <macro>`. |
| Arrow keys don't advance a lesson | The console has focus; press Esc to close it, or click the model. |
| Caption text was split in two | Put the caption text in quotes. |
| Clicking stops selecting parts | A `wait click` is still waiting; click again, press →, or type `stop`. |
| Something runs that you didn't mean | `stop` cancels everything at once. |
| "macro …" by voice runs nothing | Check the console: *no macro sounds like "…"* lists the names it knows. Say "macro" first, then the name; test with `say "macro <name>"`. |

**Voice.** Voice commands run through the same system, and the console shows how each phrase was heard. For now voice recognises the original command set (show/hide systems, heart, brain, lungs, liver, stomach, focus, zoom, rotate, slice, reset…) plus "macro <name>" to run any macro. Any other part name is reached by typing, or by putting it in a macro. Use `say "…"` to test how a phrase would be understood without a microphone.

**Known issues in the existing app.** `reset` logs an error from the cross-section gizmo (`CrossPlanesGizmo.ResetAll`) partway through: the body, selection and camera are restored, but the reset of the slice controls and colour keys after that point is skipped. Inside a macro the next line runs normally. Stopping Play mode logs a `MissingReferenceException` from `CrossSections`; it is harmless.

**Not built yet:** `repeat N` blocks, a spoken "stop", voice for any part name and any command, quizzes, and floating name tags on parts.
