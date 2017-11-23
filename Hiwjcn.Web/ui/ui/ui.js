function reload_parent() {
    if (window && window.parent) {
        window.parent.location.reload();
    }
    else {
        alert('无法刷新父级页面');
    }
}

function get(url, data, success) {
    ajax(url, data, 'get', success, function () {
        //
    });
}
function postFormJson(url, formID, success) {
    postJson(url, $('#' + formID).serializeArray(), success);
}
function post(url, data, success) {
    ajax(url, data, 'post', success, function () {
        //
    });
}
function getJson(url, data, success) {
    sendJson(url, data, 'GET', success);
}
function postJson(url, data, success) {
    sendJson(url, data, 'POST', success);
}
function sendJson(url, data, method, success) {
    sendAjax({
        url: url,
        data: data,
        dataType: 'json',
        method: method,
        success: success,
        error: function () {
            //
        }
    });
}
function ajax(url, data, method, success, complete) {
    sendAjax({
        url: url,
        data: data,
        method: method,
        success: success,
        error: function () {
            //
        },
        complete: complete
    });
}
function sendAjax(param) {
    if (param == null || typeof param == 'undefined') { return; }
    var url, data, dataType, method, success, error, before, complete;
    url = param.url ? param.url : window.location.href;
    data = param.data ? param.data : {};
    dataType = param.dataType ? param.dataType : 'html';
    method = param.method ? param.method : 'get';
    success = (param.success && typeof param.success == 'function') ? param.success : null;
    error = (param.error && typeof param.error == 'function') ? param.error : null;
    before = (param.before && typeof param.before == 'function') ? param.before : null;
    complete = (param.complete && typeof param.complete == 'function') ? param.complete : null;
    $.ajax({
        type: method,
        url: url,
        data: data,
        dataType: dataType,
        beforeSend: function (XMLHttpRequest) {
            if (before != null) { before(); }
        },
        success: function (data, textStatus) {
            if (success != null) { success(data); }
        },
        complete: function (XMLHttpRequest, textStatus) {
            if (complete != null) { complete(); }
        },
        error: function () {
            if (error != null) { error(); }
        }
    });
}

function formJson(id, callback) {
    var fm = $('#' + id);
    var url = fm.attr('action');
    var method = fm.attr('method').toUpperCase();
    var data = fm.serializeArray();
    $.ajax({
        type: method,
        url: url,
        data: data,
        dataType: 'JSON',
        success: function (data, textStatus) {
            if (callback != null) {
                callback(data);
            }
        },
        error: function () {
            alert('请求失败');
        }
    });
}

/*
 * Lazy Load - jQuery plugin for lazy loading images
 *
 * Copyright (c) 2007-2013 Mika Tuupola
 *
 * Licensed under the MIT license:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 * Project home:
 *   http://www.appelsiini.net/projects/lazyload
 *
 * Version:  1.8.4
 *
 */
/*! Lazy Load 1.9.5 - MIT license - Copyright 2010-2015 Mika Tuupola */
!function (a, b, c, d) { var e = a(b); a.fn.lazyload = function (f) { function g() { var b = 0; i.each(function () { var c = a(this); if (!j.skip_invisible || c.is(":visible")) if (a.abovethetop(this, j) || a.leftofbegin(this, j)); else if (a.belowthefold(this, j) || a.rightoffold(this, j)) { if (++b > j.failure_limit) return !1 } else c.trigger("appear"), b = 0 }) } var h, i = this, j = { threshold: 0, failure_limit: 0, event: "scroll", effect: "show", container: b, data_attribute: "original", skip_invisible: !1, appear: null, load: null, placeholder: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAANSURBVBhXYzh8+PB/AAffA0nNPuCLAAAAAElFTkSuQmCC" }; return f && (d !== f.failurelimit && (f.failure_limit = f.failurelimit, delete f.failurelimit), d !== f.effectspeed && (f.effect_speed = f.effectspeed, delete f.effectspeed), a.extend(j, f)), h = j.container === d || j.container === b ? e : a(j.container), 0 === j.event.indexOf("scroll") && h.bind(j.event, function () { return g() }), this.each(function () { var b = this, c = a(b); b.loaded = !1, (c.attr("src") === d || c.attr("src") === !1) && c.is("img") && c.attr("src", j.placeholder), c.one("appear", function () { if (!this.loaded) { if (j.appear) { var d = i.length; j.appear.call(b, d, j) } a("<img />").bind("load", function () { var d = c.attr("data-" + j.data_attribute); c.hide(), c.is("img") ? c.attr("src", d) : c.css("background-image", "url('" + d + "')"), c[j.effect](j.effect_speed), b.loaded = !0; var e = a.grep(i, function (a) { return !a.loaded }); if (i = a(e), j.load) { var f = i.length; j.load.call(b, f, j) } }).attr("src", c.attr("data-" + j.data_attribute)) } }), 0 !== j.event.indexOf("scroll") && c.bind(j.event, function () { b.loaded || c.trigger("appear") }) }), e.bind("resize", function () { g() }), /(?:iphone|ipod|ipad).*os 5/gi.test(navigator.appVersion) && e.bind("pageshow", function (b) { b.originalEvent && b.originalEvent.persisted && i.each(function () { a(this).trigger("appear") }) }), a(c).ready(function () { g() }), this }, a.belowthefold = function (c, f) { var g; return g = f.container === d || f.container === b ? (b.innerHeight ? b.innerHeight : e.height()) + e.scrollTop() : a(f.container).offset().top + a(f.container).height(), g <= a(c).offset().top - f.threshold }, a.rightoffold = function (c, f) { var g; return g = f.container === d || f.container === b ? e.width() + e.scrollLeft() : a(f.container).offset().left + a(f.container).width(), g <= a(c).offset().left - f.threshold }, a.abovethetop = function (c, f) { var g; return g = f.container === d || f.container === b ? e.scrollTop() : a(f.container).offset().top, g >= a(c).offset().top + f.threshold + a(c).height() }, a.leftofbegin = function (c, f) { var g; return g = f.container === d || f.container === b ? e.scrollLeft() : a(f.container).offset().left, g >= a(c).offset().left + f.threshold + a(c).width() }, a.inviewport = function (b, c) { return !(a.rightoffold(b, c) || a.leftofbegin(b, c) || a.belowthefold(b, c) || a.abovethetop(b, c)) }, a.extend(a.expr[":"], { "below-the-fold": function (b) { return a.belowthefold(b, { threshold: 0 }) }, "above-the-top": function (b) { return !a.belowthefold(b, { threshold: 0 }) }, "right-of-screen": function (b) { return a.rightoffold(b, { threshold: 0 }) }, "left-of-screen": function (b) { return !a.rightoffold(b, { threshold: 0 }) }, "in-viewport": function (b) { return a.inviewport(b, { threshold: 0 }) }, "above-the-fold": function (b) { return !a.belowthefold(b, { threshold: 0 }) }, "right-of-fold": function (b) { return a.rightoffold(b, { threshold: 0 }) }, "left-of-fold": function (b) { return !a.rightoffold(b, { threshold: 0 }) } }) }(jQuery, window, document);

$(function () {
    //图片懒加载
    $('img.lazy').lazyload({
        effect: 'fadeIn'
    });
    //panel折叠
    var panels = $(".panel-collapsible").find('.panel-heading');
    panels.each(function (i) {
        var panel = panels.eq(i);
        var open = true;
        panel.click(function () {
            if (open) {
                panel.next().slideUp();
            }
            else {
                panel.next().slideDown();
            }
            open = !open;
        });
    });
    //弹出框
    if (window.layer || $.layer) {
        var wins = $('.layer_win');
        wins.each(function (i) {
            var win = wins.eq(i);
            win.click(function () {
                var url = $(this).attr('href');
                var title = $(this).attr('title');
                layer.open({
                    type: 2,
                    title: title,
                    shadeClose: true,
                    shade: 0.8,
                    area: ['90%', '98%'],
                    content: url
                });
                return false;
            });
        });
    }
    //select自动选中
    var selects = $('select');
    selects.each(function (i) {
        var select = selects.eq(i);
        var val = select.attr('data-value');
        if (val == undefined || val == 'undefined' || val == null) {
            return false;
        }
        select.val(val);
    });
    //提示tip
    $('[data-toggle="tooltip"]').tooltip()

});

try
{
    console.warn('请注意敏感信息保密')
}
catch(e)
{
    //
}