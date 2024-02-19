import('System.BitConverter')

Convert = {}

Convert['IntToByteArray'] = function(int)
	-- if we use BitConverter.GetBytes it'll call the double version because lua sucks
	return ReverseArray(IntToBytes(int))
end

Convert['ByteArrayToInt'] = function(bytes)
	return BitConverter.ToUInt32(ReverseArray(bytes))
end

Convert.FloatToByteArray = function(f)
	return FloatToBytes(f)
end