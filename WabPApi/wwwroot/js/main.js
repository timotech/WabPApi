
(function ($) {
    "use strict";
    var currentPageNumber = 1;
    GetCart();

    /*[ Load page ]
    ===========================================================*/
    $(".animsition").animsition({
        inClass: 'fade-in',
        outClass: 'fade-out',
        inDuration: 1500,
        outDuration: 800,
        linkElement: '.animsition-link',
        loading: true,
        loadingParentElement: 'html',
        loadingClass: 'animsition-loading-1',
        loadingInner: '<div class="loader05"></div>',
        timeout: false,
        timeoutCountdown: 5000,
        onLoadEvent: true,
        browser: [ 'animation-duration', '-webkit-animation-duration'],
        overlay : false,
        overlayClass : 'animsition-overlay-slide',
        overlayParentElement : 'html',
        transition: function(url){ window.location.href = url; }
    });
    
    /*[ Back to top ]
    ===========================================================*/
    var windowH = $(window).height()/2;

    $(window).on('scroll',function(){
        if ($(this).scrollTop() > windowH) {
            $("#myBtn").css('display','flex');
        } else {
            $("#myBtn").css('display','none');
        }
    });

    $('#myBtn').on("click", function(){
        $('html, body').animate({scrollTop: 0}, 300);
    });


    /*==================================================================
    [ Fixed Header ]*/
    var headerDesktop = $('.container-menu-desktop');
    var wrapMenu = $('.wrap-menu-desktop');

    if($('.top-bar').length > 0) {
        var posWrapHeader = $('.top-bar').height();
    }
    else {
        var posWrapHeader = 0;
    }
    

    if($(window).scrollTop() > posWrapHeader) {
        $(headerDesktop).addClass('fix-menu-desktop');
        $(wrapMenu).css('top',0); 
    }  
    else {
        $(headerDesktop).removeClass('fix-menu-desktop');
        $(wrapMenu).css('top',posWrapHeader - $(this).scrollTop()); 
    }

    $(window).on('scroll',function(){
        if($(this).scrollTop() > posWrapHeader) {
            $(headerDesktop).addClass('fix-menu-desktop');
            $(wrapMenu).css('top',0); 
        }  
        else {
            $(headerDesktop).removeClass('fix-menu-desktop');
            $(wrapMenu).css('top',posWrapHeader - $(this).scrollTop()); 
        } 
    });


    /*==================================================================
    [ Menu mobile ]*/
    $('.btn-show-menu-mobile').on('click', function(){
        $(this).toggleClass('is-active');
        $('.menu-mobile').slideToggle();
    });

    var arrowMainMenu = $('.arrow-main-menu-m');

    for(var i=0; i<arrowMainMenu.length; i++){
        $(arrowMainMenu[i]).on('click', function(){
            $(this).parent().find('.sub-menu-m').slideToggle();
            $(this).toggleClass('turn-arrow-main-menu-m');
        })
    }

    $(window).resize(function(){
        if($(window).width() >= 992){
            if($('.menu-mobile').css('display') == 'block') {
                $('.menu-mobile').css('display','none');
                $('.btn-show-menu-mobile').toggleClass('is-active');
            }

            $('.sub-menu-m').each(function(){
                if($(this).css('display') == 'block') { console.log('hello');
                    $(this).css('display','none');
                    $(arrowMainMenu).removeClass('turn-arrow-main-menu-m');
                }
            });
                
        }
    });


    /*==================================================================
    [ Show / hide modal search ]*/
    $('.js-show-modal-search').on('click', function(){
        $('.modal-search-header').addClass('show-modal-search');
        $(this).css('opacity','0');
    });

    $('.js-hide-modal-search').on('click', function(){
        $('.modal-search-header').removeClass('show-modal-search');
        $('.js-show-modal-search').css('opacity','1');
    });

    $('.container-search-header').on('click', function(e){
        e.stopPropagation();
    });


    /*==================================================================
    [ Isotope ]*/
    var $topeContainer = $('.isotope-grid');
    var $filter = $('.filter-tope-group');

    // filter items on button click
    $filter.each(function () {
        $filter.on('click', 'button', function () {
            var filterValue = $(this).attr('data-filter');
            $topeContainer.isotope({filter: filterValue});
        });
        
    });

    // init Isotope
    $(window).on('load', function () {
        var $grid = $topeContainer.each(function () {
            $(this).isotope({
                itemSelector: '.isotope-item',
                layoutMode: 'fitRows',
                percentPosition: true,
                animationEngine : 'best-available',
                masonry: {
                    columnWidth: '.isotope-item'
                }
            });
        });
    });

    var isotopeButton = $('.filter-tope-group button');

    $(isotopeButton).each(function(){
        $(this).on('click', function(){
            for(var i=0; i<isotopeButton.length; i++) {
                $(isotopeButton[i]).removeClass('how-active1');
            }

            $(this).addClass('how-active1');
        });
    });

    /*==================================================================
    [ Filter / Search product ]*/
    $('.js-show-filter').on('click',function(){
        $(this).toggleClass('show-filter');
        $('.panel-filter').slideToggle(400);

        if($('.js-show-search').hasClass('show-search')) {
            $('.js-show-search').removeClass('show-search');
            $('.panel-search').slideUp(400);
        }    
    });

    $('.js-show-search').on('click',function(){
        $(this).toggleClass('show-search');
        $('.panel-search').slideToggle(400);

        if($('.js-show-filter').hasClass('show-filter')) {
            $('.js-show-filter').removeClass('show-filter');
            $('.panel-filter').slideUp(400);
        }    
    });




    /*==================================================================
    [ Cart ]*/
    $('.js-show-cart').on('click',function(){
        $('.js-panel-cart').addClass('show-header-cart');
    });

    $('.js-hide-cart').on('click',function(){
        $('.js-panel-cart').removeClass('show-header-cart');
    });

    /*==================================================================
    [ Cart ]*/
    $('.js-show-sidebar').on('click',function(){
        $('.js-sidebar').addClass('show-sidebar');
    });

    $('.js-hide-sidebar').on('click',function(){
        $('.js-sidebar').removeClass('show-sidebar');
    });

    /*==================================================================
    [ +/- num product ]*/
    $('.btn-num-product-down').on('click', function(){
        var numProduct = Number($(this).next().val());
        if(numProduct > 0) $(this).next().val(numProduct - 1);
    });

    $('.btn-num-product-up').on('click', function(){
        var numProduct = Number($(this).prev().val());
        $(this).prev().val(numProduct + 1);
    });

    /*==================================================================
    [ Rating ]*/
    $('.wrap-rating').each(function(){
        var item = $(this).find('.item-rating');
        var rated = -1;
        var input = $(this).find('input');
        $(input).val(0);

        $(item).on('mouseenter', function(){
            var index = item.index(this);
            var i = 0;
            for(i=0; i<=index; i++) {
                $(item[i]).removeClass('zmdi-star-outline');
                $(item[i]).addClass('zmdi-star');
            }

            for(var j=i; j<item.length; j++) {
                $(item[j]).addClass('zmdi-star-outline');
                $(item[j]).removeClass('zmdi-star');
            }
        });

        $(item).on('click', function(){
            var index = item.index(this);
            rated = index;
            $(input).val(index+1);
        });

        $(this).on('mouseleave', function(){
            var i = 0;
            for(i=0; i<=rated; i++) {
                $(item[i]).removeClass('zmdi-star-outline');
                $(item[i]).addClass('zmdi-star');
            }

            for(var j=i; j<item.length; j++) {
                $(item[j]).addClass('zmdi-star-outline');
                $(item[j]).removeClass('zmdi-star');
            }
        });
    });
    
    /*==================================================================
    [ Show modal1 ]*/
    $('.js-show-modal1').on('click',function(e){
        e.preventDefault();
        $('.js-modal1').addClass('show-modal1');
        //$('.prev1').addClass('show-img');

        var selectedImage = $(this).parent('.hov-img0').find('img:first');
        var slectedImageUrl = selectedImage.attr('src');               
        $('#imgPic1').attr('src', slectedImageUrl);

        var productId = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Id"]').val();
        $('#hidId').val(productId);

        //First Thumb
        $('ul.slick3-dots').children().first().children().attr('src', slectedImageUrl);
        
        var productName = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="ProdName"]');
        var selectedName = productName.attr('value');
        $('.mtext-105').html(selectedName);

        var productPrice = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Price"]');
        var selectedPrice = productPrice.attr('value');
        $('.mtext-106').html(selectedPrice);

        var productDescription = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Description"]');
        var selectedDescription = productDescription.attr('value');
        $('.stext-102').html(selectedDescription);

        //var productSize = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Size"]');
        //var selectedSize = productSize.attr('value');
        //$('.mtext-107').html(selectedSize);

        var selectedImage2 = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Image2"]');
        var selectedImage2Url = selectedImage2.attr('value');
        $('#imgPic2').attr('src', selectedImage2Url);

        //Second Thumb
        $('ul.slick3-dots li:nth-child(2)').children().attr('src', selectedImage2Url)

        var selectedImage3 = $(this).parent('.hov-img0').next(".block2-txt").find('input[name="Image3"]');
        var selectedImage3Url = selectedImage3.attr('value');
        $('#imgPic3').attr('src', selectedImage3Url);

        //Third Thumb
        $('ul.slick3-dots li:nth-child(3)').children().attr('src', selectedImage3Url)
    });

    $('.js-hide-modal1').on('click',function(){
        $('.js-modal1').removeClass('show-modal1');
    });

    //Add to Cart
    $('.js-addcart-detail').click(function () {
        var id = $('#hidId').val();
        var prodName = $('.mtext-105').text();
        var numItems = parseInt($('input[name="num-product"]').val(),10);
        var price = $('.mtext-106').text();        
        var cartCount = parseInt($('.icon-header-item.cl2.hov-cl1.trans-04.p-l-22.p-r-11.icon-header-noti.js-show-cart').attr('data-notify'), 10);
        swal({
            title: "Defendis Hair",
            text: "Please Wait While Adding " + prodName + " to cart!!!",
            icon: "info",
            buttons: false
        }); 

        $.post("/ShoppingCart/AddToCart", { "id": id, "Quantity": numItems },
            function (data) {
                //$("#spinner").hide();
                var result = "";                
                $(data).each(function (index, emp) {                    
                    result += '<li class="header-cart-item flex-w flex-t m-b-12"><div class="header-cart-item-img">                          <img src="/images/' + emp.cartItems[0].product.picPathMain + '" alt="IMG"></div><div class="header-cart-item-txt p-t-8"><a href="#" class="header-cart-item-name m-b-18 hov-cl1 trans-04">' + emp.cartItems[0].product.name + '</a> <span class="header-cart-item-info">' + numItems + ' x N' + emp.cartItems[0].product.price +  '</span>  </div> </li>';
                });
                $('ul.header-cart-wrapitem.w-full').append(result);
                cartCount += numItems;
                $('.icon-header-item.cl2.hov-cl1.trans-04.p-l-22.p-r-11.icon-header-noti.js-show-cart').attr('data-notify', cartCount);
                //vatDiv.html('<b>VAT</b> :' + data.Vat);
                $('.header-cart-total.w-full.p-tb-40').html('Total: N' + data.cartTotal);
                
                swal(prodName, "is added to cart !", "success");                
                
            });
    });

    function GetCart() {
        var cartCount = parseInt($('.icon-header-item.cl2.hov-cl1.trans-04.p-l-22.p-r-11.icon-header-noti.js-show-cart').attr('data-notify'), 10);

        $.ajax({
            type: 'GET',
            url: '/ShoppingCart/GetCartDetails',
            success: function (data) {
                var result = "";
                $('ul.header-cart-wrapitem.w-full').html('');
                $(data).each(function (index, emp) {
                    result += '<li class="header-cart-item flex-w flex-t m-b-12"><div class="header-cart-item-img">                          <img src="/images/' + emp.cartItems[0].product.picPathMain + '" alt="IMG"></div><div class="header-cart-item-txt p-t-8"><a href="#" class="header-cart-item-name m-b-18 hov-cl1 trans-04">' + emp.cartItems[0].product.name + '</a> <span class="header-cart-item-info">' + emp.cartItems[0].count + ' x N' + emp.cartItems[0].product.price + '</span>  </div> </li>';
                    cartCount += emp.cartItems[0].count;
                });
                $('ul.header-cart-wrapitem.w-full').append(result);
                
                $('.icon-header-item.cl2.hov-cl1.trans-04.p-l-22.p-r-11.icon-header-noti.js-show-cart').attr('data-notify', cartCount);
                //vatDiv.html('<b>VAT</b> :' + data.Vat);
                $('.header-cart-total.w-full.p-tb-40').html('Total: N' + data.cartTotal);
			}
        });
    }

    //$('.flex-c-m.stext-101.cl5.size-103.bg2.bor1.hov-btn1.p-lr-15.trans-04').on('click', function (e) {
    //    e.preventDefault();        
    //    loadMore(currentPageNumber);
    //    currentPageNumber += 1;        
    //});

   // function loadMore(currentPage) {
   //     $.ajax({
   //         url: '/Home/GetMoreProducts',
   //         method: 'post',
   //         data: { count: currentPage },
   //         dataType: 'json',
   //         beforeSend: function () {
   //             $('.flex-c-m.stext-101.cl5.size-103.bg2.bor1.hov-btn1.p-lr-15.trans-04').html('');
   //             $('.flex-c-m.stext-101.cl5.size-103.bg2.bor1.hov-btn1.p-lr-15.trans-04').append('<span class="spinner-border spinner-border-sm"></span>  Loading..');
   //         },
   //         success: function (data) {
   //             var result = "";                
   //             $(data).each(function (index, emp) {
   //                 result += '<div class="col-sm-6 col-md-4 col-lg-3 p-b-35 isotope-item ' + emp.description2 + '"><div class="block2"><div class="block2-pic hov-img0"><div class="block2-pic hov-img0"><img id="imgPix2" src="' + emp.picPathMain + '" alt="IMG"><a href="#" class="block2-btn flex-c-m stext-103 cl2 size-102 bg0 bor2 hov-btn1 p-lr-15 trans-04 js-show-modal1">Quick View</a ></div><div class="block2-txt flex-w flex-t p-t-14"><div class="block2-txt-child1 flex-col-l "><a href="/Home/ProductDetail/' + emp.id + '" class="stext-104 cl4 hov-cl1 trans-04 js-name-b2 p-b-6">' + emp.name + '</a><input id="Id" name="Id" type="hidden" value="' + emp.ID + '"><input id="Image1" name="Image1" type="hidden" value="' + emp.picPathMain + '"><input id="Image2" name="Image2" type="hidden" value="' + emp.picPathLeft + '"><input id="Image3" name="Image3" type="hidden" value="' + emp.picPathRight + '"><input id="Description" name="Description" type="hidden" value="' + emp.description + '"><input id="Price" name="Price" type="hidden" value="' + emp.price + '"><input id="Size" name="Size" type="hidden" value="' + emp.size + '"><input id="ProdName" name="ProdName" type="hidden" value="' + emp.name + '"><input id="Category" name="Category" type="hidden" value="' + emp.description2 + '"><span class="stext-105 cl3">' + 'N' + emp.price + '</span><div class="block2-txt-child2 flex-r p-t-3"><a href ="#" class="btn-addwish-b2 dis-block pos-relative js-addwish-b2"><img class="icon-heart1 dis-block trans-04" src="/images/icons/icon-heart-01.png" alt="ICON">  <img class="icon-heart2 dis-block trans-04 ab-t-l" src="/images/icons/icon-heart-02.png" alt="ICON"></a></div></div></div></div>';
   //             });
                
   //             $('.row.isotope-grid').removeAttr("style");
   //             $('.isotope-item').removeAttr("style");
   //             $('.row.isotope-grid').append(result).fadeIn(4000);

   //         },
   //         complete: function () {
   //             $('.flex-c-m.stext-101.cl5.size-103.bg2.bor1.hov-btn1.p-lr-15.trans-04').html('Load More');
			//}
   //     });
   // }

})(jQuery);