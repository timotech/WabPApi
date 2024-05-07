function payWithPaystack() {
    var email = document.getElementById('inputEmail').innerHTML;
    var amount = document.getElementById('inputAmount').value;
    amount = amount * 100;

    var phone = document.getElementById('inputPhone').innerHTML;

    var handler = PaystackPop.setup({
        key: 'pk_live_f0c98efb8c712c09935eaf2fc3bedb9d6df0b1f9', //put your public key here

        email: email, //put your customer's email here

        amount: amount, //amount the customer is supposed to pay

        metadata: {

            custom_fields: [

                {

                    display_name: "Mobile Number",

                    variable_name: "mobile_number",

                    value: phone //"+2348012345678" //customer's mobile number

                }

            ]

        },

        callback: function (response) {

            //after the transaction have been completed

            //make post call  to the server with to verify payment 

            //using transaction reference as post data
            //alert('success. transaction ref is ' + response.reference);

            $.get('/CheckOut/Verify', { reference: response.reference }, function (status) {
                if (status == "success") {
                    //successful transaction

                    alert('Transaction was successful');
                    window.location = "//www.WabPApi.com.ng/CheckOut/OrderComplete?Ref=" + response.reference;
                }
                else {
                    //transaction failed
                    alert(response);
                    window.location = "//www.WabPApi.com.ng/CheckOut/OrderFailure?Ref=" + response.reference;
				}
            });
        },
        onClose: function () {
            //when the user close the payment modal
            alert('Payment cancelled');
        }
    });
    handler.openIframe(); //open the paystack's payment modal
}