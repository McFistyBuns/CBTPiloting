# CBT Piloting

CBT Piloting attempts to bring Classic Battletech Tabletop piloting skill checks into HBS's BATTLETECH game.  Currently only one check is implemented.

Whenever you mech becomes unstable, every hit that causes stability damage will cause a Piloting skill check.  Currently the base difficulty is a flat 30%.  On damage, the game will roll a random number and apply your piloting skill.  Under 30% will cause a knockdown regradless of your total stability damage.  The piloting skill percentage is calculated the way all skill check percentages are calculated in the game, which is to take your skill and divide it by the skill divisor ( Skill / PilotingDivisor).  The default PilotingDivisor is 40.  So for example, a piloting skill of 5 will add a 12.5% chance to the random roll.  The default difficulty of 30% leaves a pilot with a skill of 10 a 5% chance of failure.  I figured that was a good trade-off, since the CBT piloting skill checks always had a chance of failure no matter the skill level.

Difficulty percentage is configurable in the mod.json file.

## Installation

Install [BTML](https://github.com/Mpstark/BattleTechModLoader) and [ModTek](https://github.com/Mpstark/ModTek). Extract files to `BATTLETECH\Mods\CBTPiloting\`.
