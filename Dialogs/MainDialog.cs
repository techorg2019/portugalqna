// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using SNOW.Logger;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

     //   public RecognizerResult Recognizerresult { get; }

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger/*, RecognizerResult luisresult*/)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;
           // Recognizerresult = luisresult;
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new BookingDialog());
            AddDialog(new IncDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
                //ActStepAsync,
                //FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Configuration["LuisAppId"]) || string.IsNullOrEmpty(Configuration["LuisAPIKey"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostName"]))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);


                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {



                var bookingDetails = stepContext.Result == null
                  ?
              await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, /*Recognizerresult,*/ cancellationToken)
                  :
              new BookingDetails();

                //await stepContext.Context.SendActivityAsync(
                //  MessageFactory.Text("Incident Created..!"), cancellationToken);
                //return await stepContext.EndDialogAsync(null, cancellationToken);
                //  return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);



                //   return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"") }, cancellationToken);
             

                 if (bookingDetails.Create_incident.Equals("true"))
                {
                    //     var bookingDetails = stepContext.Result != null
                    //    ?
                    //await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken)
                    //    :
                    //new BookingDetails();

                    //await stepContext.Context.SendActivityAsync(
                    //  MessageFactory.Text("Incident Created..!"), cancellationToken);
                    //return await stepContext.EndDialogAsync(null, cancellationToken);

                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);


                } else  if (bookingDetails.None.Equals("true"))
                {
                    bookingDetails.None = "false";
                    CoreBot.models.apiresult incidentno = null;
                    SNOWLogger nOWLogger = new SNOWLogger(Configuration);
                    string concat = "";
                    String userinput = stepContext.Context.Activity.Text.Trim();
                    if (!(userinput.ToUpper().Contains("KB0")))
                    {
                        incidentno = nOWLogger.KBSearchServiceNow("GOTO123TEXTQUERY321=" + stepContext.Context.Activity.Text.Trim());

                        if (incidentno.result != null)
                        {

                            if (incidentno.result.Count != 0)
                            {




                                                                                                                                                                                 
                                for (int i = 0; i < incidentno.result.Count; i++)
                                {
                                    concat += "\n" + incidentno.result[i].number + " : " + incidentno.result[i].short_description + "\n";
                                }


                                await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text(concat), cancellationToken);

                            }
                            else


                            {
                                //                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(" Sorry no details found for " + stepContext.Context.Activity.Text) }, cancellationToken);

                                await stepContext.Context.SendActivityAsync(
                                MessageFactory.Text(" Sorry no details found for: " + stepContext.Context.Activity.Text.Trim()), cancellationToken);

                                return await stepContext.EndDialogAsync(null, cancellationToken);


                            }

                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(
                          MessageFactory.Text(" Sorry no details found"), cancellationToken);

                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        }

                    }







                    // string concat = string.Join(" ", myList.ToArray());

                    //  return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(incidentno.SelectToken("result[" + i + "].number").ToString() + incidentno.SelectToken("result[" + i + "].short_description").ToString()) }, cancellationToken);
                    // return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(concat) }, cancellationToken);


                    stepContext.Context.TurnState.Add("incID", bookingDetails);

                    return await stepContext.NextAsync(bookingDetails, cancellationToken);
                }
                
                else
                {
                    stepContext.Context.TurnState.Add("incID", bookingDetails);

                    return await stepContext.NextAsync(bookingDetails, cancellationToken);

                }


            }

        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //   var bookingDetails = (BookingDetails)stepContext.Options;

            // bookingDetails.TravelDate = (string)stepContext.Result;

            //            var msg = $"Please confirm, I have you traveling to: {bookingDetails.Destination} from: {bookingDetails.Origin} on: {bookingDetails.TravelDate}";
            // CoreBot.models.apiresult incidentno = null;
            SNOWLogger nOWLogger = new SNOWLogger(Configuration);
            String userinput = stepContext.Context.Activity.Text.Trim();
            //  var bookingDetails = (BookingDetails)stepContext.Options;

            if ((BookingDetails)stepContext.Result != null)
            {
                var bookingDetails = (BookingDetails)stepContext.Context.TurnState["incID"];




                if ((userinput.ToUpper().Contains("KB0")))
                {
                    CoreBot.models.apiresult kbresult = nOWLogger.KBSearchByNumber(stepContext.Context.Activity.Text.Trim());



                    string concatkb = "";

                    if (kbresult.result.Count != 0)
                    {


                        for (int i = 0; i < kbresult.result.Count; i++)
                        {
                            concatkb += kbresult.result[i].number + " : " + kbresult.result[i].short_description + "\n" + kbresult.result[i].text + "\n";
                        }
                    }
                    else


                    {
                        //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(" Sorry no detials found for KB No: " + stepContext.Context.Activity.Text) }, cancellationToken);
                        await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(" Sorry no detials found for KB No: " + stepContext.Context.Activity.Text.Trim()), cancellationToken);


                        //await stepContext.EndDialogAsync(null, cancellationToken);

                        // return await stepContext.NextAsync(null, cancellationToken);
                    }

                    //  return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(concatkb) }, cancellationToken);

                    await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(concatkb), cancellationToken);
                    //  var msg = $"Are you satisfied with the information? ";

                    //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);

                    //await stepContext.NextAsync(stepContext, cancellationToken);

                    var msg = $"Are you satisfied with the information? \n Select \"No\" to create Incident ";

                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);


                }
                else

                if (bookingDetails.Incident_status.Equals("true"))
                {
                    bookingDetails.Incident_status = "false";

                    //  SNOWLogger nOWLoggerforinc = new SNOWLogger(Configuration);





                    // string concat = "";
                    if (bookingDetails != null)
                    {
                        return await stepContext.BeginDialogAsync(nameof(IncDialog), bookingDetails, cancellationToken);

                    }



                    return await stepContext.NextAsync(stepContext, cancellationToken);
                    // return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                }

            }else
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);

            }
            


            return await stepContext.NextAsync(stepContext, cancellationToken);




        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

                if (!(stepContext.Context.Activity.Text.Trim().ToUpper().Contains("KB0")))
            {

                if (stepContext.Result != null)
               {

                    if (stepContext.Result.Equals(true))
                    {
                        // var bookingDetails = (BookingDetails)stepContext.Options;

                        //return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text("Please enter any other query."), cancellationToken);

                        // return await stepContext.EndDialogAsync(null, cancellationToken);
                        return await stepContext.NextAsync(null, cancellationToken);

                    }

                    else if (stepContext.Result.Equals(false))
                    {
                        var bookingDetails = stepContext.Result != null
                       ?
                   await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context,/* Recognizerresult,*/ cancellationToken)
                       :
                   new BookingDetails();

                        //await stepContext.Context.SendActivityAsync(
                        //  MessageFactory.Text("Incident Created..!"), cancellationToken);
                        //return await stepContext.EndDialogAsync(null, cancellationToken);
                        return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);
                    } 
                    else {
                        return await stepContext.EndDialogAsync(null, cancellationToken);

                    }



                } else
                {
                    return await stepContext.EndDialogAsync(null, cancellationToken);

                }

            }
            return await stepContext.EndDialogAsync(null, cancellationToken);

        }

            

           
        




        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {



            if (stepContext.Result.Equals(null))
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);


            }

            if ((bool)stepContext.Result)
            {
                // var bookingDetails = (BookingDetails)stepContext.Options;

                //return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Any thing else I Can Help you with ?"), cancellationToken);
                

            }
            else
            {

                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Incident Created..!"), cancellationToken);
                                
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
            //   var msg = $"Are you satisfied with the input? ";

            //  return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);

            //return await stepContext.NextAsync(null, cancellationToken);

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            //var bookingDetails = stepContext.Result != null
            //        ?
            //    await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken)
            //        :
            //    new BookingDetails();

            // In this sample we only have a single Intent we are concerned with. However, typically a scenario
            // will have multiple different Intents each corresponding to starting a different child Dialog.

            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.

            //if ((bool)stepContext.Result)
            //{
            //    // var bookingDetails = (BookingDetails)stepContext.Options;

            //    //return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("Incident Created..!"), cancellationToken);
            //}
            //else
            //{

            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("Any thing elase I Can Help you with ?"), cancellationToken);
            //    return await stepContext.EndDialogAsync(null, cancellationToken);
            //}




            //            if (stepContext.Result != null)
            //            {
            //                var result = (BookingDetails)stepContext.Result;

            //                // Now we have all the booking details call the booking service.

            //                // If the call to the booking service was successful tell the user.

            //                var timeProperty = new TimexProperty(result.TravelDate);
            //                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            ////                var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //                var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";


            //                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            //            }
            //            else
            //            {
            //                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            //            }
            //            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);

        }
    }
}
