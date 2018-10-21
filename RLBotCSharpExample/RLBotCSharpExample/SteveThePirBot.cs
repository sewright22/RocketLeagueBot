using rlbot.flat;
using RLBotDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RLBotCSharpExample
{
    public class SteveThePirBot : Bot
    {
        private readonly Vector3 _ownGoalLocation;
        private readonly Vector3 _opponentGoalLocation;
        private bool _haveReachedGoal;
        private bool _haveHitBall;

        public SteveThePirBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
        {
            _ownGoalLocation = this.GetFieldInfo().Goals(botTeam).Value.Location.Value;
            _opponentGoalLocation = this.GetFieldInfo().Goals(1).Value.Location.Value;
            _haveHitBall = false;
            _haveReachedGoal = true;
        }

        public override Controller GetOutput(GameTickPacket gameTickPacket)
        {
            // This controller object will be returned at the end of the method.
            // This controller will contain all the inputs that we want the bot to perform.
            Controller controller = new Controller();

            // Wrap gameTickPacket retrieving in a try-catch so that the bot doesn't crash whenever a value isn't present.
            // A value may not be present if it was not sent.
            // These are nullables so trying to get them when they're null will cause errors, therefore we wrap in try-catch.
            try
            {
                // Store the required data from the gameTickPacket.
                Vector3 ballLocation = gameTickPacket.Ball.Value.Physics.Value.Location.Value;
                Vector3 botLocation = gameTickPacket.Players(this.index).Value.Physics.Value.Location.Value;
                Rotator botRotation = gameTickPacket.Players(this.index).Value.Physics.Value.Rotation.Value;

                double botDistanceToBall = GetDistanceToTarget(ballLocation, botLocation);
                double botToBallAngle = GetAngleToBall(ballLocation, botLocation);
                double botFrontToBallAngle = botToBallAngle - botRotation.Yaw;

                double botToGoalAngle = Math.Atan2(_opponentGoalLocation.Y - botLocation.Y, _opponentGoalLocation.X - botLocation.X);
                double botFrontToGoalAngle = botToGoalAngle - botRotation.Yaw;

                // Correct the angle
                if (botFrontToGoalAngle < -Math.PI)
                {
                    botFrontToGoalAngle += 2 * Math.PI;
                }

                if (botFrontToGoalAngle > Math.PI)
                {
                    botFrontToGoalAngle -= 2 * Math.PI;
                }

                // Decide which way to steer in order to get to the ball.
                if (botFrontToGoalAngle > 0)
                {
                    controller.Steer = 1;
                }
                else
                {
                    controller.Steer = -1;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            // Set the throttle to 1 so the bot can move.
            controller.Throttle = 1;

            return controller;
        }

        private double GetDistanceToTarget(Vector3 ballLocation, Vector3 botLocation)
        {
            return Math.Sqrt(Math.Pow(ballLocation.Y-botLocation.Y, 2) + Math.Pow(ballLocation.X-botLocation.X,2));
        }

        private static double GetAngleToBall(Vector3 ballLocation, Vector3 carLocation)
        {
            // Calculate to get the angle from the front of the bot's car to the ball.
            return Math.Atan2(ballLocation.Y - carLocation.Y, ballLocation.X - carLocation.X);
        }
    }
}
