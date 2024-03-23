mergeInto(LibraryManager.library,{   
    //Unity内自定义调用方法名 Vue_Upload(string json) 
    Vue_Upload_Unity: function (json) { 
     strs = Pointer_stringify(json);   //字符串需用Pointer_stringify转换
     Vue_Upload(strs);                        //前端自定义方法名GetScore(strs)
    },    
});