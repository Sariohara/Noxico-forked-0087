function Excite(this)
	local char = this.Character
	if char.HasToken("beast") or char.HasToken("sleeping") then return end
--	if player.ParentBoard ~= this.ParentBoard
--		player = nil
--	local ogled = false
	local aroundMe = this.GetCharsWithin(4)
	if #aroundMe == 0 then return end
	for i=0,#aroundMe-1,1 do
		local other = aroundMe[i]
		if other.Character.HasToken("beast") then
			-- nothing
		elseif not this.CanSee(other) then
			-- nothing
		elseif not this.Character.Likes(other.Character) then
			return -- wait
		elseif other.Character.GetStat("charisma") >= 10 then
			-- print(this.ToString() .. " is maybe excited by " .. other.ToString())
			local stim = this.Character.GetStat("excitement")
			local theirChar = other.Character.GetStat("charisma")
			local distance = other.DistanceFrom(this)
			local increase = (theirChar / 20) * (distance * 0.25)
			-- print(this.ToString() .. " is excited for " .. increase .. " points by " .. other.ToString())
			this.Character.Raise("excitement", increase)
			if distance < 2 then
				-- print(this.ToString() .. " is proximity-excited for 0.25 points by " .. other.ToString())
				this.Character.Raise("excitement", 0.25)
			end
			-- TODO: ogle
		end
	end
end

--[[
public void CheckForCopiers()
{
	if (Character.HasToken("copier"))
	{
		var copier = Character.GetToken("copier");
		var timeout = copier.GetToken("timeout");
		if (timeout != null && timeout.Value > 0)
		{
			if (!timeout.HasToken("minute"))
				timeout.AddToken("minute", NoxicoGame.InGameTime.Minute);
			if (timeout.GetToken("minute").Value == NoxicoGame.InGameTime.Minute)
				return;
			timeout.GetToken("minute").Value = NoxicoGame.InGameTime.Minute;
			timeout.Value--;
			if (timeout.Value == 0)
			{
				copier.RemoveToken(timeout);
				if (copier.HasToken("full") && copier.HasToken("backup"))
				{
					Character.Copy(null); //force revert
					AdjustView();
					NoxicoGame.AddMessage(i18n.GetString("x_reverts").Viewpoint(Character));
				}
			}
		}
	}
}

(When porting this to Lua, note that this came from the Character class, not BoardChar.
public void UpdateOviposition()
{
	if (BoardChar == null)
		return;
	if (this.HasToken("egglayer") && this.HasToken("vagina"))
	{
		var eggToken = this.GetToken("egglayer");
		eggToken.Value++;
		if (eggToken.Value == 500)
		{
			eggToken.Value = 0;
			var egg = new DroppedItem("egg")
			{
				XPosition = BoardChar.XPosition,
				YPosition = BoardChar.YPosition,
				ParentBoard = BoardChar.ParentBoard,
			};
			egg.Take(this, BoardChar.ParentBoard);
			if (BoardChar is Player)
				NoxicoGame.AddMessage(i18n.GetString("youareachicken").Viewpoint(this));
			return;
		}
	}
	return;
}
]]--

function EachBoardCharTick(who, char)
	Excite(who)
--	CheckForCopiers(who)
--	UpdateOviposition(who)
end

function EndPlayerTurn()
	Excite(player)
end